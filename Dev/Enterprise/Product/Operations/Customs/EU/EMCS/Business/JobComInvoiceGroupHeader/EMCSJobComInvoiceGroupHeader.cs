using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.EU.EMCS.Business
{
	public class EMCSJobComInvoiceGroupHeader : BaseJobComInvoiceGroupHeader, Integration.Customs.EUEMCS.IJobComInvoiceGroupHeader
	{
		public EMCSJobComInvoiceGroupHeader(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region New

		public new EMCSJobDeclaration JobDeclaration => (EMCSJobDeclaration)base.JobDeclaration;

		public new EMCSJobComInvoiceGroupHeader GroupHeader => (EMCSJobComInvoiceGroupHeader)base.GroupHeader;

		public new EMCSInvoiceHeaderActiveCollection JobComInvoiceHeaders => (EMCSInvoiceHeaderActiveCollection)base.JobComInvoiceHeaders;

		#endregion

		#region Overridden

		protected override IJobComInvoiceGroupHeaderCollection<BaseJobComInvoiceGroupHeader> CreateNewJobComInvoiceGroupHeaderCollection() => new BaseJobComInvoiceGroupHeaderCollection<EMCSJobComInvoiceGroupHeader>(this);

		protected override InvoiceHeaderActiveCollection CreateNewDirectInvoiceHeaderCollection() => new EMCSInvoiceHeaderActiveCollection(this, true);

		#endregion

		#region Type Decider

		public new static readonly EMCSJobComInvoiceGroupHeaderTypeDecider TypeDecider = new EMCSJobComInvoiceGroupHeaderTypeDecider();

		public class EMCSJobComInvoiceGroupHeaderTypeDecider : BaseJobComInvoiceGroupHeaderTypeDecider
		{
			public override Type GetTypeForLoad(DataRow row, BusinessObjectFactory factory)
			{
				return typeof(EMCSJobComInvoiceGroupHeader);
			}

			public override Type GetTypeForBinding()
			{
				return typeof(EMCSJobComInvoiceGroupHeader);
			}

			protected override Type GetTypeForNewCore(ITypeDeciderContext context)
			{
				return typeof(EMCSJobComInvoiceGroupHeader);
			}

			protected override Type DefaultTypeForUnsupportedCountry
			{
				get { return typeof(EMCSJobComInvoiceGroupHeader); }
			}
		}

		#endregion
	}
}
