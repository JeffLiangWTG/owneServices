using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngine.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Services.OperationalActions.Business
{
	sealed class DeliverableDocumentCommandCollection : StmMenuItemBaseCollection
	{
		public DeliverableDocumentCommandCollection(BusinessObjectFactory factory, ZString businessContext, ZString staffNK, bool onlySystemDefined)
			: base(factory, GenerateFilter(businessContext, staffNK, onlySystemDefined))
		{
			this.businessContext = businessContext;
			this.staffNK = staffNK;
			this.onlySystemDefined = onlySystemDefined;

			FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Business Context", "Property", businessContext));
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
		public new DocumentCommand this[int index]
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return (DocumentCommand)base[index]; }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
		[System.Diagnostics.DebuggerStepThrough]
		public new DocumentCommand AddNew()
		{
			return (DocumentCommand)base.AddNew();
		}

		protected override void AddNotificationWhenAdditionalFilterNotMet(StringCollectionX errors, BusinessObject selectedBusinessObject)
		{
			base.AddNotificationWhenAdditionalFilterNotMet(errors, selectedBusinessObject);

			DocumentCommand command = (DocumentCommand)selectedBusinessObject;

			if (command.SU_BusinessContext != businessContext)
			{
				errors.Add(Res.GetString("3c7d3759-b539-4c9b-839f-7abee21f5fcc", "This document has the wrong business context."));
			}

			if (!command.SU_GS_NKStaffCode.IsEmpty)
			{
				if (staffNK.IsEmpty)
				{
					errors.Add(Res.GetString("f222cc19-4620-4467-86bf-3c334d1eefa3", "Only published documents may be selected."));
				}
				else if (command.SU_GS_NKStaffCode != staffNK)
				{
					errors.Add(Res.GetString("7fb4ba6e-155a-4bf8-9a70-82c0da51a2b5", "Unpublished documents belonging to other users may not be selected."));
				}
			}

			if (command.SU_ContactType == ContactType.NoContactType.Code)
			{
				errors.Add(Res.GetString("961b17e6-61ba-4a90-95ac-029137ad80db", "Only documents with a contact type may be selected."));
			}

			if (onlySystemDefined)
			{
				if (!command.SU_IsSystemDefined)
				{
					errors.Add(Res.GetString("27553977-28ba-41c5-bb78-aa47f94db75e", "Only system defined documents may be selected."));
				}
				else if (command.SU_IsClientSpecific)
				{
					errors.Add(Res.GetString("468feae8-39fc-4ad0-baf9-03eee0bf371a", "Client specific documents may not be selected."));
				}
			}
		}

		static ZQuery GenerateFilter(ZString businessContext, ZString staffNK, bool onlySystemDefined)
		{
			ZQuery result = new ZQuery();
			result.AddToFilter(StmMenuItemSchema.SU_BusinessContext, businessContext);

			if (onlySystemDefined)
			{
				result.AddToFilter(StmMenuItemSchema.SU_IsSystemDefined, true);
				result.AddToFilter(StmMenuItemSchema.SU_IsClientSpecific, false);
			}

			if (staffNK.IsEmpty)
			{
				result.AddToFilter(StmMenuItemSchema.SU_GS_NKStaffCode, "");
			}
			else
			{
				ZQuery staffQuery = new ZQuery();
				staffQuery.DefaultJoinCondition = JoinCondition.Or;
				staffQuery.AddToFilter(StmMenuItemSchema.SU_GS_NKStaffCode, "");
				staffQuery.AddToFilter(StmMenuItemSchema.SU_GS_NKStaffCode, staffNK);

				result.AddToFilter(staffQuery);
			}

			return result;
		}

		readonly bool onlySystemDefined;
		readonly ZString businessContext;
		readonly ZString staffNK;
	}
}
