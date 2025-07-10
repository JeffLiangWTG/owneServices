using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using CargoWise.Common.Enumeration;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Services.OperationalActions.Support.Testing
{
	[TestsSubclassesOf(typeof(OperationalActionSupporter))]
	public abstract class OperationalActionSupporterTest<SupporterT> : TestCaseWithFactory where SupporterT : OperationalActionSupporter
	{
		public void TestMustNotHoldAReferenceToTheModule()
		{
			WeakReference weakModuleRef;
			OperationalActionSupporter supporter = GetSupporterFromModuleID(out weakModuleRef, ModuleID);
			GC.Collect();
			GC.WaitForPendingFinalizers();
			GC.Collect();
			GC.WaitForPendingFinalizers();
			const string message = "The action supporter must not hold a reference to the module as there are some cases\n" + "where the action supporter is held around much longer than the module that supplied it.\n" + "";
			AssertEquals(message, false, weakModuleRef.IsAlive);
		}

		[System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
		static OperationalActionSupporter GetSupporterFromModuleID(out WeakReference weakModuleRef, ModuleIdentifier id)
		{
			using (ZFilterGridModule module = (ZFilterGridModule)ZModuleFactory.Instance.Create(id))
			{
				weakModuleRef = new WeakReference(module);
				IOperationalActionSupportable supportable = (IOperationalActionSupportable)module;
				return supportable.OperationalActionSupporter;
			}
		}

		[SuppressMessage("CargoWiseOne", "CW1115:DoNotUseSetUserContext", Justification = "Baseline")]
		public void TestSecurityCheckPoint()
		{
			// Bug found in Work Item WI00112341, if you dont clear the context then suspected leaks from previous tests will make the securities DisplayText have then incorrect value (DAT only). Will be resolved soon
			var ctx = Env.CurrentUserContext;
			Env.ClearUserContext();
			Env.SetUserContext(ctx);
			AssertSecurityCheckpoint(Supporter.CustomizationSecurityCheckpoint, ExpectedCustomizationSecurityCheckpoint, SecurityCore.Captions.CustomiseActions, "CustomizationSecurityCheckpoint");
			AssertSecurityCheckpoint(Supporter.RunSecurityCheckpoint, ExpectedRunSecurityCheckpoint, SecurityCore.Captions.RunActions, "RunSecurityCheckpoint");
		}

		public void AssertSecurityCheckpoint(SecurityCheckpoint checkPoint, SecurityCheckpoint expectedCheckpoint, string caption, string checkPointNameForMessage)
		{
			AssertNotNull("Should provide a security checkpoint", checkPoint);
			AssertEquals(expectedCheckpoint, checkPoint);
			AssertEquals("Module security checkpoint should have the given name", caption, checkPoint.DisplayText);

			if (checkPoint.Parent.Parent != ExpectedCheckpointGrandparent)
			{
				var builder = new StringBuilder().Append(string.Format("The {0} needs to be a grandchild checkpoint of the module security checkpoint and needs to be named \"{1}\".</br>", checkPointNameForMessage, caption)).Append(HtmlFormatGoodValue(ExpectedCheckpointGrandparent, (c) => Render(c, "*", caption))).Append(HtmlFormatBadValue(checkPoint, (c) => Render(c)));
				HtmlFail(builder.ToString());
			}
		}

		protected virtual SecurityCheckpoint ExpectedCheckpointGrandparent
		{
			get
			{
				return Module.SecurityCheckpoint;
			}
		}

		protected virtual SecurityCheckpoint ExpectedCustomizationSecurityCheckpoint
		{
			get
			{
				return Env.Security.FindOrCreateOperationalActionsCustomiseCheckpoint(Supporter.BaseCheckpoint);
			}
		}

		protected virtual SecurityCheckpoint ExpectedRunSecurityCheckpoint
		{
			get
			{
				return Env.Security.FindOrCreateOperationalActionsRunCheckpoint(Supporter.BaseCheckpoint);
			}
		}

		protected virtual SecurityCheckpoint ExpectedAllowRunOnAllMatchingRecordsSecurityCheckpoint
		{
			get
			{
				return Env.Security.FindOrCreateOperationalActionsAllowRunOnAllMatchingRecordsCheckpoint(Supporter.BaseCheckpoint);
			}
		}

		public virtual void TestBusinessContextIsConsistent()
		{
			ExtraTestProvider.TestBusinessContextIsConsistent();
		}

		public virtual void TestDocumentBusinessContextIsConsistent()
		{
			ExtraTestProvider.TestDocumentBusinessContextIsConsistent();
		}

		public virtual void TestModuleCorrectlySupportsOperationalActions()
		{
			ExtraTestProvider.TestModuleCorrectlySupportsOperationalActions();
		}

		public void TestActionsOnlyReferenceFieldsThatExist()
		{
			ExtraTestProvider.TestActionsOnlyReferenceFieldsThatExist();
		}

		public void TestActionsOnlyReferenceMethodsThatExist()
		{
			ExtraTestProvider.TestActionsOnlyReferenceMethodsThatExist();
		}

		public void TestMethodSettingsPassValidation()
		{
			ExtraTestProvider.TestAllMethodSettingsPassValidation();
		}

		public void TestActionFiltersAreValid()
		{
			ExtraTestProvider.TestActionFiltersAreValid();
		}

		public void TestFieldDefaultsAreValid()
		{
			ExtraTestProvider.TestFieldDefaultsAreValid();
		}

		public void TestExistingActionsPassValidation()
		{
			ExtraTestProvider.TestExistingActionsPassValidation();
		}

		#region Implementation
		public virtual bool ShouldSupportDocuments
		{
			get
			{
				return true;
			}
		}

		public virtual BusinessObject NewTarget()
		{
			return Module.GridCollection.AddNew();
		}

		public new BusinessObjectFactory Factory
		{
			[System.Diagnostics.DebuggerStepThrough]
			get
			{
				return base.Factory;
			}
		}

		public ZFilterGridModule Module
		{
			get
			{
				return module ?? (module = (ZFilterGridModule)ZModuleFactory.Instance.Create(ModuleID));
			}
		}

		ZFilterGridModule module;
		public SupporterT Supporter
		{
			get
			{
				return supporter ?? (supporter = NewSupporter());
			}
		}

		protected virtual SupporterT NewSupporter()
		{
			IOperationalActionSupportable supportable = Module as IOperationalActionSupportable;
			OperationalActionSupporter supporter = supportable == null ? null : supportable.OperationalActionSupporter;
			return supporter as SupporterT;
		}

		SupporterT supporter;
		IExtraTestProvider ExtraTestProvider
		{
			get
			{
				if (extraTestProvider == null)
				{
					Type genericTypeDefinition = Type.GetType(ExtraTestProviderTypeName);
					Type genericType = genericTypeDefinition.MakeGenericType(typeof(SupporterT));
					extraTestProvider = (IExtraTestProvider)Activator.CreateInstance(genericType, this);
				}

				return extraTestProvider;
			}
		}

		IExtraTestProvider extraTestProvider;
		const string ExtraTestProviderTypeName = "Enterprise.Services.OperationalActions.GUI.Testing.OperationalActionSupporterExtraTestProvider`1,Enterprise.Services.OperationalActions.GUI.Test";
		protected override void TearDown()
		{
			using (module)
			{
				base.TearDown();
			}
		}

		protected abstract ModuleIdentifier ModuleID { get; }

		string Render(SecurityCheckpoint checkpoint, params string[] childNames)
		{
			var displayNames = ZEnumerable.Iterate(checkpoint, c => c.Parent).TakeWhile(c => c != null).Select(c => c.DisplayText.ToString()).Reverse().Concat(childNames);
			return string.Join(" -> ", displayNames);
		}

		#endregion
		#region IExtraTestProvider
		/// <summary>
		/// An interface for running tests that require knowledge of objects
		/// defined in Enterprise.Services.OperationalActions.Business.
		/// </summary>
		public interface IExtraTestProvider
		{
			void TestModuleCorrectlySupportsOperationalActions();
			void TestBusinessContextIsConsistent();
			void TestDocumentBusinessContextIsConsistent();
			void TestActionsOnlyReferenceFieldsThatExist();
			void TestActionsOnlyReferenceMethodsThatExist();
			void TestAllMethodSettingsPassValidation();
			void TestActionFiltersAreValid();
			void TestFieldDefaultsAreValid();
			void TestExistingActionsPassValidation();
		}
		#endregion
	}
}
