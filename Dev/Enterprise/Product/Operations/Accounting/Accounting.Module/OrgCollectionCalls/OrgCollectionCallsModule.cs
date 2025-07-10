using System;
using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using CreditOptions = Enterprise.Accounting.Business.ARAP.Invoicing.Statement.CreditOptions;
using StatementCollectionLetterType = Enterprise.Core.Constants.StatementCollectionLetterType;

namespace Enterprise.Accounting.Module
{
	public class OrgCollectionCallsModule : ZFilterGridModule
	{
		public OrgCollectionCallsModule()
		{
		}

		#region Overrides

		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.OrgCollectionCalls; }
		}

		protected override MenuItem[] GetNewStandardMenuItems()
		{
			List<MenuItem> menuItems = new List<MenuItem>(base.GetNewStandardMenuItems());

			menuItems.Remove(NewMenuItem);
			menuItems.Remove(DeleteMenuItem);

			return menuItems.ToArray();
		}

		protected override MenuItem[] GetNewActionMenuItems()
		{
			List<MenuItem> menuItems = new List<MenuItem>(base.GetNewActionMenuItems());

			MenuItem[] items = new MenuItem[5];
			items[0] = new ZMenuItem(PrintStatementMenuItemText, new EventHandler(HandlePrintStatement));
			items[1] = new ZMenuItem(PrintFirstReminderMenuItemText, new EventHandler(HandlePrintFirstReminder));
			items[2] = new ZMenuItem(PrintSecondReminderMenuItemText, new EventHandler(HandlePrintSecondReminder));
			items[3] = new ZMenuItem(PrintCollectionLetterMenuItemText, new EventHandler(HandlePrintCollectionLetter));
			items[4] = new ZMenuItem(PrintDemandLetterMenuItemText, new EventHandler(HandlePrintDemandLetter));

			KMenuItem printDocumentsMenuItem = new ZMenuItem(PrintDocumentsMenuItemText, items);
			menuItems.Insert(1, printDocumentsMenuItem);

			return menuItems.ToArray();
		}

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return ZControllerFactory.Create(ControllerIDs.OrgCollectionCalls);
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new OrgCollectionCallsFilterControl(GridCollection, (OrgCollectionCallsFilterBusinessObject)FilterBusinessObject);
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new OrgCollectionCallCollection(Factory);
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new OrgCollectionCallsFilterBusinessObject();
		}

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get { return Env.Security.ReceivablesCollectionCalls; }
		}

		protected override LicenceCheckpoint LicenceCheckPointCore
		{
			get { return Env.Licence.Core; }
		}

		#endregion

		#region Implementation

		protected static MultilingualString PrintDocumentsMenuItemText
		{
			get { return ResString.GetMultilingualString("47d9cd7b-8510-41fd-9b8e-4c52fb656516", "&Print Documents"); }
		}
		protected static MultilingualString PrintStatementMenuItemText
		{
			get { return ResString.GetMultilingualString("6aee4a32-9fd3-4bff-9242-ad6baa26b036", "Print S&tatement Of Account"); }
		}
		protected static MultilingualString PrintFirstReminderMenuItemText
		{
			get { return ResString.GetMultilingualString("c7fac399-7b9f-4ade-b6b1-808702780e5c", "Print First &Reminder"); }
		}
		protected static MultilingualString PrintSecondReminderMenuItemText
		{
			get { return ResString.GetMultilingualString("6db868a3-e0f6-49c0-8b6a-93c91d37b9c4", "Print &Second Reminder"); }
		}
		protected static MultilingualString PrintCollectionLetterMenuItemText
		{
			get { return ResString.GetMultilingualString("bf060d0a-4e4b-42a3-b104-12c1ce3dca3e", "Print &Collection Letter"); }
		}
		protected static MultilingualString PrintDemandLetterMenuItemText
		{
			get { return ResString.GetMultilingualString("68a0d35c-e88c-4c4d-b273-00083335d8a5", "Print &Demand Letter"); }
		}

		protected void HandlePrintStatement(object sender, EventArgs e)
		{
			PrintDocument(StatementCollectionLetterType.StatementOfAccount);
		}

		protected void HandlePrintFirstReminder(object sender, EventArgs e)
		{
			PrintDocument(StatementCollectionLetterType.FirstReminder);
		}

		protected void HandlePrintSecondReminder(object sender, EventArgs e)
		{
			PrintDocument(StatementCollectionLetterType.SecondReminder);
		}

		protected void HandlePrintCollectionLetter(object sender, EventArgs e)
		{
			PrintDocument(StatementCollectionLetterType.CollectionLetter);
		}

		protected void HandlePrintDemandLetter(object sender, EventArgs e)
		{
			PrintDocument(StatementCollectionLetterType.DemandLetter);
		}

		protected void PrintDocument(ZString documentToPrint)
		{
			if (SecurityCheckpoint != null)
			{
				if (!SecurityCheckpoint.IsAllowed)
				{
					SecurityCheckpoint.ShowError();
				}
				else
				{
					GlbBranch currentGlbBranch = Factory.LoadTop1<GlbBranch>(new ZQuery(GlbBranchSchema.PK, Env.CurrentBranch.PK));
					Statement statement = Statement.New(currentGlbBranch);
					if (statement.DocumentToPrint == documentToPrint)
					{
						statement.ValidateDocumentToPrint();
					}
					else
					{
						statement.DocumentToPrint = documentToPrint;
					}
					statement.CreditStatements = CreditOptions.AllDocuments;

					if (statement.DocumentToPrintInfo.HasErrors())
					{
						Globals.Message.ShowError(statement.DocumentToPrintInfo.GetErrors().GetFirstMessage());
					}
					else
					{
						OrgCollectionCallCollection selectedCalls = new OrgCollectionCallCollection(Factory);
						selectedCalls.AddRange(this.Grid.SelectedElements);

						foreach (OrgCollectionCall call in selectedCalls)
						{
							statement.BatchOfOrganisationsToPrint.Add(call.Header.PK);
						}

						if (statement.BatchOfOrganisationsToPrint.Count > 0)
						{
							statement.PrintStatements();
						}
					}
				}
			}
		}

		protected SecurityCheckpoint GetSecurityCheckPointForDocument(ZString documentToPrint)
		{
			SecurityCheckpoint checkpoint = null;

			switch (documentToPrint)
			{
				case StatementCollectionLetterType.StatementOfAccount:
					checkpoint = Env.Security.ReceivablesPrintStatement;
					break;

				case StatementCollectionLetterType.FirstReminder:
					checkpoint = Env.Security.ReceivablesPrintFirstReminder;
					break;

				case StatementCollectionLetterType.SecondReminder:
					checkpoint = Env.Security.ReceivablesPrintSecondReminder;
					break;

				case StatementCollectionLetterType.CollectionLetter:
					checkpoint = Env.Security.ReceivablesPrintCollectionLetter;
					break;

				case StatementCollectionLetterType.DemandLetter:
					checkpoint = Env.Security.ReceivablesPrintDemandLetter;
					break;
			}

			return checkpoint;
		}

		#endregion
	}
}
