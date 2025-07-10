using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.PlugIn;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Modules.Testing
{
	[TestsSubclassesOf(typeof(ZController), ExcludeClientDlls = true)]
	public abstract class ZControllerBasherTest : CountrySpecificTest
	{
		[SnailTest]
		[RequiresSTA]
		public virtual void TestNewForm()
		{
			AssertControllerNotNull();
			try
			{
				AssertNotNull(Controller.ShowNewForm());
			}
			catch (ModuleFeatureNotSupportedException)
			{
			}
		}

		[SnailTest]
		[RequiresSTA]
		public virtual void TestViewForm()
		{
			AssertControllerNotNull();
			try
			{
				AssertNotNull(Controller.ShowViewForm(GetBusinessObjectThatIsInTheDatabase()));
			}
			catch (ModuleFeatureNotSupportedException)
			{
			}
		}

		[SnailTest]
		[RequiresSTA]
		public virtual void TestEditForm()
		{
			AssertControllerNotNull();
			IZForm testForm = null;

			try
			{
				testForm = GetEditFormToShow();
				AssertNotNull(testForm);
			}
			catch (ModuleFeatureNotSupportedException)
			{
			}
			finally
			{
				var testZForm = testForm as ZForm;
				if (testZForm != null)
				{
					testZForm.Close();
				}
			}
		}

		[SnailTest]
		[RequiresSTA]
		public virtual void TestDeleteForm()
		{
			AssertControllerNotNull();
			try
			{
				var bizO = GetBusinessObjectThatIsInTheDatabase();
				var checkpoint = Controller.GetCheckPointForDelete(bizO);
				var form = Controller.ShowDeleteForm(bizO);
				if (checkpoint.IsAllowed && bizO.CanDelete)
				{
					AssertNotNull(form);
				}
				else
				{
					AssertNull(form);
				}
			}
			catch (ModuleFeatureNotSupportedException)
			{
			}
		}

		[SnailTest]
		[RequiresSTA]
		public virtual void TestTemplateCopyForm()
		{
			AssertControllerNotNull();

			try
			{
				IBusiness businessObject = GetBusinessObjectThatIsInTheDatabase();
				if (businessObject is ITemplateCopyable)
				{
					Controller.ShowTemplateCopyForm((BusinessObject)businessObject);
				}
			}
			catch (ModuleFeatureNotSupportedException)
			{
			}
		}

		#region TestSaveFormWithPlugIns

		[SnailTest]
		[RequiresSTA]
		public virtual void TestSaveFormWithCustomsPlugIns()
		{
			AssertControllerNotNull();

			var currentCompany = new BusinessObjectFactory().Load<IGlbCompany>(StaticCurrentFetcher.Instance.CurrentCompany.PK);
			var currentCountryCode = currentCompany.Country.RN_Code;
			IEnumerable<string> countryCodesHavePlugIns = new[] { currentCountryCode.ToString() };
			IEnumerable<ControllerID> allPlugIns;
			IEnumerable<ControllerInfo> allControlInfos;
			BusinessObject bizO;
			var controllerList = new ControllerList();

			try
			{
				if (Controller.TypeOfTopLevelBusinessObject == null)
				{
					return;
				}

				bizO = GetBusinessObjectThatIsInTheDatabase();
				if (bizO == null)
				{
					return;
				}

				using (var form = Controller.ShowEditForm(bizO) as ZForm)
				{
					if (form == null)
					{
						return;
					}
					if (form.DisplayMode != Core.ODisplayMode.Edit && form.DisplayMode != Core.ODisplayMode.Browse)
					{
						form.Close();
						return;
					}

					allPlugIns = GetAllPlugInsFromControl(form);
					allControlInfos = controllerList.All.Where(controllerInfo => allPlugIns.Any(plugIn => plugIn == controllerInfo.ID));
					form.Close();
				}
			}
			catch (NotImplementedException)
			{
				return;
			}
			catch (NotSupportedException)
			{
				return;
			}
			catch (ZException)
			{
				return;
			}

			if (!allControlInfos.Any(x => x.AssemblyNameForTest.StartsWith("Enterprise.Customs")))
			{
				return;
			}

			var isCountrySpecificController = controllerList.All.Any(x => x.ID == Controller.ID && !string.IsNullOrEmpty(x.CountryCodeForTest));
			if (!isCountrySpecificController)
			{
				countryCodesHavePlugIns = allControlInfos.Where(controllerInfo => !string.IsNullOrEmpty(controllerInfo.CountryCodeForTest))
						.GroupBy(c => c.ClassFullNameForTest).Select(g => g.OrderBy(c => c.CountryCodeForTest).First())
						.Select(controllerInfo => controllerInfo.CountryCodeForTest).Distinct();
			}

			foreach (var countryCode in countryCodesHavePlugIns)
			{
				using (currentCompany.TemporarilySetCountry(countryCode))
				{
					bizO = GetBusinessObjectWithoutValidationErrors();
					bizO.RunPreSaveValidation();
					AssertEquals("Should be error free", string.Empty, string.Join("\r\n", bizO.NotificationsIncludingChildren.GetErrors().GetUniqueMessageList().ToArray()));
					Factory.Save();

					using (var form = Controller.ShowEditForm(bizO) as ZForm)
					{
						var plugInToTest = allControlInfos.Where(c => string.IsNullOrEmpty(c.CountryCodeForTest) || c.CountryCodeForTest == countryCode)
							.Select(c => c.ID).Except(NonCustomsPlugInsToExcludeFromTest).ToList();
						EnableAllPlugInsFromControl(form, plugInToTest);
						form.Show();
						UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
						UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
						AssertNoExceptionThrown(() => form.FireSaveButton());
						form.Close();

						AssertEquals($@"Below plugins are not tested. Mostly this is because you put business logic conditions outside PlugIns.Add() statement.
					Please move your condition into the ChangeTheVisibility() method of these concrete PlugIns, otherwise, override NonCustomsPlugInsToExcludeFromTest and add the untested PlugIns into it.
					{string.Join(", ", plugInToTest)}", 0, plugInToTest.Count);
					}
				}
			}
		}

		protected virtual BusinessObject GetBusinessObjectWithoutValidationErrors()
		{
			return GetBusinessObjectThatIsInTheDatabase();
		}

		/// <summary>
		/// Override this property, if some plug ins are unnecessary to test.
		/// </summary>
		protected virtual IEnumerable<ControllerID> NonCustomsPlugInsToExcludeFromTest => new ControllerID[] { ControllerIDs.Audit, ControllerIDs.JobInvoicing, ControllerIDs.ETailShipment };

		IEnumerable<ControllerID> GetAllPlugInsFromControl(Control control)
		{
			var result = new List<ControllerID>();
			if (control is ZForm)
			{
				result.AddRange(GetAllPlugIns(((ZForm)control).PlugIns));
			}
			else if (control is ZTabControl)
			{
				result.AddRange(GetAllPlugIns(((ZTabControl)control).PlugIns));
			}

			foreach (Control subControl in control.Controls)
			{
				result.AddRange(GetAllPlugInsFromControl(subControl));
			}
			return result;
		}

		IEnumerable<ControllerID> GetAllPlugIns(PlugIns plugIns)
		{
			return (GetFieldValue(plugIns, "PlugInInfos", true) as ArrayList).ToArray().Select(o => GetFieldValue(o, "ID") as ControllerID);
		}

		object GetFieldValue(object target, string fieldName, bool isNonPublic = false)
		{
			var field = GetFieldInfo(ref target, fieldName, isNonPublic);
			return field.GetValue(target);
		}

		FieldInfo GetFieldInfo(ref object target, string fieldName, bool isNonPublic)
		{
			var fieldPath = fieldName.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
			var field = target.GetType().GetField(fieldPath[0], BindingFlags.Instance | (isNonPublic ? BindingFlags.NonPublic : BindingFlags.Public));

			if (fieldPath.Length == 1)
			{
				return field;
			}
			else
			{
				var subFieldPath = string.Join(".", fieldPath.Skip(1));
				target = field.GetValue(target);

				return GetFieldInfo(ref target, subFieldPath, isNonPublic);
			}
		}

		void EnableAllPlugInsFromControl(Control control, List<ControllerID> plugInsToEnable)
		{
			if (control is ZForm)
			{
				EnableAllPlugIns(((ZForm)control).PlugIns, plugInsToEnable);
			}
			else if (control is ZTabControl)
			{
				EnableAllPlugIns(((ZTabControl)control).PlugIns, plugInsToEnable);
			}

			if (plugInsToEnable.Count > 0)
			{
				foreach (Control subControl in control.Controls)
				{
					EnableAllPlugInsFromControl(subControl, plugInsToEnable);
				}
			}
		}

		void EnableAllPlugIns(PlugIns plugIns, List<ControllerID> plugInsToEnable)
		{
			foreach (var plugIn in plugIns.Instances)
			{
				plugIn.Enabled = true;

				if (plugInsToEnable.Contains(plugIn.ControllerID))
				{
					plugInsToEnable.Remove(plugIn.ControllerID);
				}
				if (NonCustomsPlugInsToExcludeFromTest.Contains(plugIn.ControllerID))
				{
					plugIn.Enabled = false;
				}

				if (plugIn.Enabled)
				{
					plugIn.Setup();
				}
			}
		}

		#endregion

		[RequiresSTA]
		public virtual void TestGetOpenFormUrlslDoesNotHitDatabase()
		{
			IZForm testForm = null;

			try
			{
				testForm = GetEditFormToShow();
				if (testForm is ZForm)
				{
					_ = CWNextFeatureHelper.IsCWNextEnabled(); // Pre-invoke IsCWNextEnabled
					_ = StaticCurrentFetcher.Instance.CurrentCompany?.LicenceKeyIdentifier; // Pre-invoke lazy loaded property
					_ = DataRegistry.Instance.AddDatabaseInfoToEdientUrls; // Pre-invoke registry
					using (Db.Connection.TrackExecutedCommands(includeStackTrace: true))
					{
						WindowPersister.GetOpenFormUrls();
						AssertContainsExactElementsInAnyOrder(
							"GetOpenFormUrls will be called when restarting CW1 after a DB upgrade so it should not hit the DB, executed commands",
							Array.Empty<string>(), Db.Connection.ExecutedCommands);
					}
				}
				else
				{
					Assert(true);
				}
			}
			catch (ModuleGuiNotSupportedException)
			{
				Assert(true);
			}
			catch (NotSupportedException)
			{
				Assert(true);
			}
			finally
			{
				CloseAndDispose(testForm);
			}
		}

		protected virtual IZForm GetEditFormToShow()
		{
			return Controller.ShowEditForm(GetBusinessObjectThatIsInTheDatabase());
		}

		protected virtual void CloseAndDispose(IZForm form)
		{
			if (form is ZForm zForm)
			{
				zForm?.Close();
			}

			form?.Dispose();
		}

		public virtual Type ControllerToBashType
		{
			get { return Controller.GetType(); }
		}

		#region Implementation

		protected ZController Controller;

		protected abstract ControllerID GetControllerID();

		protected virtual Type GetBusinessObjectType()
		{
			return Controller.TypeOfTopLevelBusinessObject;
		}

		//		[Obsolete("Put your test data setup code in BusinessObject.FillWithValidTestData and don't override this method.")]
		protected virtual BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			var result = Factory.NewWithValidTestData(GetBusinessObjectType());
			Factory.Save();
			return result;
		}

		protected override void SetUp()
		{
			base.SetUp();
			Controller = ZControllerFactory.Create(GetControllerID());
		}

		protected override void TearDown()
		{
			base.TearDown();

			if (Controller != null && Controller.LastShownForm != null)
			{
				System.Windows.Forms.Application.DoEvents();
				Controller.LastShownForm.Dispose();
			}
		}

		protected void AssertControllerNotNull()
		{
			AssertNotNull(
				@"Controller should not be null, check your ID and that you are returning the correct CountryCode.
Current Country = " + StaticCurrentFetcher.Instance.CurrentCompany.Country.RN_Code, Controller);
		}

		#endregion
	}
}
