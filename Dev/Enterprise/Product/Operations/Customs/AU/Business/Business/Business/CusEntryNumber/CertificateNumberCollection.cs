using CargoWise.EntityFramework;
using Enterprise.Customs.Common;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CertificateNumberCollection : ActiveBusinessObjectCollection<CusEntryNumber>
	{
		public CertificateNumberCollection(QuarantineExDocHeader master) : base(master.Factory, master, CreateNewRelationshipFilter(), CusEntryNumSchema.CE_ParentID) { }

		protected override void SetDefaultsForNewElementCore(CusEntryNumber newElement)
		{
			base.SetDefaultsForNewElementCore(newElement);

			newElement.CE_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			newElement.CE_ParentTable = QuarantineExDocHeader.Schema.TableName;
			newElement.CE_EntryType = CusEntryNumberTypes.Australia.QuarantineCertificateNumber;
			newElement.CE_Category = CusEntryNumber.Categories.CustomsPermitClearanceNumber;
		}

		static ZQuery CreateNewRelationshipFilter()
		{
			var result = new ZQuery();
			result.AddToFilter(CusEntryNumSchema.CE_RN_NKCountryCode, Core.Constants.CountryCodes.Australia);
			result.AddToFilter(CusEntryNumSchema.CE_ParentTable, QuarantineExDocHeader.Schema.TableName);
			result.AddToFilter(CusEntryNumSchema.CE_EntryType, CusEntryNumberTypes.Australia.QuarantineCertificateNumber);
			result.AddToFilter(CusEntryNumSchema.CE_Category, CusEntryNumber.Categories.CustomsPermitClearanceNumber);

			return result;
		}
	}
}
