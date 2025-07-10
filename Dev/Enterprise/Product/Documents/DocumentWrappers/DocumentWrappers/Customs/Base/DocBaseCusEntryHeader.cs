using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DocumentWrappers.Customs.Base
{
	public abstract class DocBaseCusEntryHeader : DocBaseWrapper, Integration.DocumentWrappers.IDocBaseCusEntryHeader
	{
		protected DocBaseCusEntryHeader(CusEntryHeader cusEntryHeader, BusinessObjectFactory factoryToWrap)
			: base(cusEntryHeader, factoryToWrap)
		{
		}

		public static DocBaseCusEntryHeader New(CusEntryHeader baseCusEntryHeader, BusinessObjectFactory factoryToWrap)
		{
			switch (baseCusEntryHeader)
			{
				case Enterprise.Customs.AU.Declaration.Business.CusEntryHeader auCusEntryHeader:
					return AU.DocCusEntryHeader.New(auCusEntryHeader, factoryToWrap);

				case Enterprise.Customs.NZ.Business.Declaration.FormalEntry.CusEntryHeader nzCusEntryHeader:
					return NZ.FormalEntry.DocCusEntryHeader.New(nzCusEntryHeader, factoryToWrap);

				default:
					return null;
			}
		}

		public override string ToString()
		{
			return EntryNumber;
		}

		#region ZDecimal Fields

		public virtual ZDecimal CustomsValue
		{
			get { return CusEntryHeader.CustomsValue; }
		}

		public ZDecimal EntryFeeAmount
		{
			get { return EntryFeeCore; }
		}

		protected virtual ZDecimal EntryFeeCore
		{
			get { return 0m; }
		}

		public ZDecimal MessageFeeAmount
		{
			get { return MessageFeeCore; }
		}

		protected virtual ZDecimal MessageFeeCore
		{
			get { return 0m; }
		}

		public ZDecimal EntryLeviesAmount
		{
			get { return EntryLeviesCore; }
		}

		protected virtual ZDecimal EntryLeviesCore
		{
			get { return 0m; }
		}

		public ZDecimal OtherChargesAmount
		{
			get { return OtherChargesCore; }
		}

		protected virtual ZDecimal OtherChargesCore
		{
			get { return 0m; }
		}

		public ZDecimal OtherEntryChargesOtherThanEntryFeeMessageFeeAndLevies
		{
			get { return OtherEntryChargesOtherThanEntryFeeMessageFeeAndLeviesCore; }
		}

		protected virtual ZDecimal OtherEntryChargesOtherThanEntryFeeMessageFeeAndLeviesCore
		{
			get { return OtherChargesAmount; }
		}

		public ZDecimal TotalAmountPayable
		{
			get { return CusEntryHeader.TotalAmountPayable; }
		}

		public ZDecimal GSTAmount
		{
			get { return CusEntryHeader.GSTAmount; }
		}

		public ZDecimal TotalPaid
		{
			get { return CusEntryHeader.CH_TotalPaid; }
		}

		public ZDecimal Weight
		{
			get { return CusEntryHeader.GrossWeight.Amount; }
		}

		public Money CIF
		{
			get { return CusEntryHeader.CIF; }
		}

		public Money CIFInLocalCurrency
		{
			get { return CusEntryHeader.CIFInLocalCurrency; }
		}
		#endregion

		#region ZInt Fields

		public ZInt Packages
		{
			get { return CusEntryHeader.PackagesCount; }
		}

		#endregion

		#region ZString Fields

		public virtual ZString EntryNumber
		{
			get { return CusEntryHeader.EntryNumber; }
		}

		public ZString BGMReference
		{
			get { return CusEntryHeader.CH_BGMReference; }
		}

		public ZString Status
		{
			get { return CusEntryHeader.CH_Status; }
		}

		public ZString WeightUQ
		{
			get { return CusEntryHeader.GrossWeight.Unit; }
		}

		#endregion

		#region Collections
		public DocOrganisationCollection Suppliers
		{
			get
			{
				return new DocOrganisationCollection(CusEntryHeader.Suppliers.Cast<OrgHeader>(), Factory);
			}
		}
		#endregion

		#region Implementation

		CusEntryHeader CusEntryHeader
		{
			get { return (CusEntryHeader)WrappedObject; }
		}

		#endregion

	}
}
