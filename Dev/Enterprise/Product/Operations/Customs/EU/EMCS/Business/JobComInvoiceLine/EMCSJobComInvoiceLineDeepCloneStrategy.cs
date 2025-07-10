using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.EU.EMCS.Business
{
	public class EMCSJobComInvoiceLineDeepCloneStrategy : JobComInvoiceLineDeepCloneStrategy
	{
		public EMCSJobComInvoiceLineDeepCloneStrategy(BaseJobComInvoiceLine invoiceLineToClone, CloneType cloneType, BaseJobComInvoiceHeader clonedInvoice, Dictionary<ZString, Dictionary<ZGuid, ZGuid>> pkPairsDictionaryCollection)
			: base(invoiceLineToClone, cloneType, clonedInvoice, pkPairsDictionaryCollection)
		{
		}

		protected override BusinessObject CloneInternal(BusinessObjectCloneArgs args)
		{
			var newLine = (EMCSJobComInvoiceLine)base.CloneInternal(args);
			var oldLine = ((EMCSJobComInvoiceLine)this.bizObjToClone);
			ClonePackageIsForInvoiceLine(oldLine, newLine, args);
			return newLine;
		}

		void ClonePackageIsForInvoiceLine(EMCSJobComInvoiceLine oldLine, EMCSJobComInvoiceLine newLine, BusinessObjectCloneArgs args)
		{
			var index = 0;

			foreach (NonPersistentPackagePivot oldPackagePivot in oldLine.EMCSPackagePivots)
			{
				using (newLine.GetValidationSuspender())
				{
					using (newLine.SuspendSettingHasChanges())
					{
						if (oldPackagePivot.IsForInvoiceLine)
						{
							var genPivot = oldLine.Factory.New<GenPivot>();
							genPivot.XX_RelationType = "EMC";
							genPivot.XX_Relation1ID = newLine.PK;
							genPivot.XX_Relation2ID = newLine.Declaration.EMCSPackages[index].PK;
							genPivot.XX_Relation1TableCode = JobComInvoiceLineSchema.Constants.Prefix;
							genPivot.XX_Relation2TableCode = CusInvPackSchema.Constants.Prefix;
						}
						index++;
					}
				}
			}
		}
	}
}
