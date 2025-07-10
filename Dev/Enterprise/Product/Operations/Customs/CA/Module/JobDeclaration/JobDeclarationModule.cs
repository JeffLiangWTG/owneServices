using System;
using System.Collections.Generic;
using System.Reflection;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Customs.CA.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Module
{
	public class JobDeclarationModule : Customs.Module.JobDeclarationModule
	{
		protected override FilterBusinessObject GetNewFilterBusinessObject() => new JobDeclarationFilterBusinessObject();

		protected override IFilterControl GetNewFilterControl() => new JobDeclarationFilterStripControl(this, GridCollection, FilterBusinessObject);

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			SetGuiProviders(Factory);
			return new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
		}

		protected override MenuItem[] GetNewStandardMenuItems()
		{
			var menus = new List<MenuItem>(base.GetNewStandardMenuItems());
			if (NewMenuItem != null)
			{
				NewMenuItem.MenuItems.Add(new ZMenuItem(ResString.GetMultilingualString("a970e6b0-afb0-4750-a798-418d03627934", "New &Declaration"),
					delegate { ShowNewForm(); }));
				NewMenuItem.MenuItems.Add(new ZMenuItem(ResString.GetMultilingualString("a5f3caf1-2f60-4199-b9d6-b41e817faf83", "New Type &F Consolidation Declaration Job"),
					delegate { ((JobDeclarationController)GetControllerForStandAlone()).ShowNewLVSForm(); }));
				NewMenuItem.MenuItems.Add(new ZMenuItem(ResString.GetMultilingualString("5FA3F059-321D-4840-AB7E-1EFEDA952A68", "&Simplified Low Value Shipment Entry Wizard"),
					delegate { ((JobDeclarationController)GetControllerForStandAlone()).ShowNewSimplifiedLVSForm(); }));
			}
			return menus.ToArray();
		}

		protected override MenuItem[] GetNewActionMenuItems()
		{
			var result = new List<MenuItem>(base.GetNewActionMenuItems());
			if (Env.CurrentUser.IsDeveloper && Env.Registry.EnableCustomsDiagnostics)
			{
				result.Add(new ZMenuItem("-"));
				result.Insert(result.Count, new ZMenuItem(ResString.GetMultilingualString("18a96fb9-9053-45a6-96cf-9600bbd0bf2c", "Run one cycle of CIG Test Helper Sender Service Task"), RunOneCIGTestSenderCycle_Click));
				result.Insert(result.Count, new ZMenuItem(ResString.GetMultilingualString("1895dba5-8cc2-4215-835b-6a3ceac94b5d", "Run one cycle of CIG Test Helper Receiver Service Task"), RunOneCIGTestReceiverCycle_Click));
			}

			result.Insert(result.Count, new ZMenuItem(CopyNewVersionForB2Helper.CopyToNewVersionForB2Caption, CopyToNewVersionForB2));
			if (UniversalReferenceConstants.IsCarmR2)
			{
				result.Insert(result.Count, new ZMenuItem(CreatePrecarmAdjustmentEntryHelper.CreatePrecarmAdjustmentEntryCaption, CreatePrecarmAdjustmentEntry));
			}
			return result.ToArray();
		}

		void CopyToNewVersionForB2(object sender, EventArgs e)
		{
			var selectedElements = GetSelectedElements();

			if (selectedElements != null && selectedElements.Length == 1)
			{
				var sourceDeclaration = (JobDeclaration)selectedElements[0];

				var copiedDeclaration = CopyNewVersionForB2Helper.CopyToNewVersionForB2(sourceDeclaration, true);
				if (copiedDeclaration != null)
				{
					((JobDeclarationController)GetControllerForStandAlone()).ShowNewCopyToB2Form(copiedDeclaration);
				}
			}
			else
			{
				Globals.Message.ShowInformation(Res.GetString("93068B3C-7C4A-4619-8A90-362C5A15DE30", "Please select one Declaration to copy."), CopyNewVersionForB2Helper.CopyToNewVersionForB2Caption);
			}
		}

		void CreatePrecarmAdjustmentEntry(object sender, EventArgs e)
		{
			var selectedElements = GetSelectedElements();
			if (selectedElements != null && selectedElements.Length == 1)
			{
				var sourceDeclaration = (JobDeclaration)selectedElements[0];
				var newDeclaration = CreatePrecarmAdjustmentEntryHelper.CreatePrecarmAdjustmentEntry(sourceDeclaration);
				if (newDeclaration != null)
				{
					((JobDeclarationController)GetControllerForStandAlone()).ShowNewPrecarmAdjustmentForm(newDeclaration);
				}
			}
			else
			{
				Globals.Message.ShowInformation(Res.GetString("81481B39-B670-491D-A303-B527DC38E445", "Please select one Declaration."), CreatePrecarmAdjustmentEntryHelper.CreatePrecarmAdjustmentEntryCaption);
			}
		}

		protected override IZForm ShowTemplateCopyForm(BusinessObject selectedBusinessObject)
		{
			var declaration = CurrentBusinessObjectInGrid as JobDeclaration;
			if (declaration != null && declaration.IsIM2)
			{
				Globals.Message.ShowError(CopyNewVersionForB2Helper.CopyNotAllowedForIM2Message, Res.GetString("5ee5d86b-19f2-4809-85f0-4c74b2258c29", "Copy not allowed"));
				return null;
			}

			return base.ShowTemplateCopyForm(selectedBusinessObject);
		}

		void RunOneCIGTestSenderCycle_Click(object sender, EventArgs e)
		{
			var type = Type.GetType("Enterprise.Customs.CA.ServiceTasks.Testing.CIGSenderTestHelper, Enterprise.Customs.CA.ServiceTasks");
			object o = Activator.CreateInstance(type, new object[] { EDIInterchange.ApplicationCodes.CAACI });
			var methodInfo = type.GetMethod("SendInterchanges", BindingFlags.Public | BindingFlags.Instance);
			methodInfo.Invoke(o, null);
			DisplayLog(o);

			o = Activator.CreateInstance(type, new object[] { EDIInterchange.ApplicationCodes.CAEXP });
			methodInfo = type.GetMethod("SendInterchanges", BindingFlags.Public | BindingFlags.Instance);
			methodInfo.Invoke(o, null);
			DisplayLog(o);

			o = Activator.CreateInstance(type, new object[] { EDIInterchange.ApplicationCodes.CAIMP });
			methodInfo = type.GetMethod("SendInterchanges", BindingFlags.Public | BindingFlags.Instance);
			methodInfo.Invoke(o, null);
			DisplayLog(o);
		}

		void RunOneCIGTestReceiverCycle_Click(object sender, EventArgs e)
		{
			var type = Type.GetType("Enterprise.Customs.CA.ServiceTasks.Testing.CIGReceiverTestHelper, Enterprise.Customs.CA.ServiceTasks");
			object o = Activator.CreateInstance(type, Array.Empty<object>());
			var methodInfo = type.GetMethod("GetInterchanges", BindingFlags.Public | BindingFlags.Instance);
			methodInfo.Invoke(o, null);
			DisplayLog(o);
		}
	}
}
