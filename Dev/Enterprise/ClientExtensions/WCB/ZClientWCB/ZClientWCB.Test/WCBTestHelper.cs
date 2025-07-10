using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ClientSharedComponents;
using Enterprise.Customs.Business;

namespace Enterprise.Client.WCB.Testing
{
	public class WCBTestHelper : SharedTestHelper
	{
		public WCBTestHelper(BusinessObjectFactory factory) : base(factory)
		{
		}

#region Set Factory Items
		internal InvHeadWithFixedInvLinesForTest InvHeadWithFixedInvLines
		{
			get
			{
				return invHeadWithFixedInvLines ?? (invHeadWithFixedInvLines = Factory.New<InvHeadWithFixedInvLinesForTest>());
			}
		}

		InvHeadWithFixedInvLinesForTest invHeadWithFixedInvLines;
		internal class InvHeadWithFixedInvLinesForTest : InvHeadWithFixedInvLines
		{
			public InvHeadWithFixedInvLinesForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			public new BaseJobComInvoiceLineViewCollection CreateNewJobComInvoiceLineCollection()
			{
				return base.CreateNewJobComInvoiceLineCollection();
			}
		}

#endregion
#region Set Registry Items
#region Set Valid Registry Items
		internal void SetValidRegistryChryslerImporter(ZGuid importerPK)
		{
			rego.ChryslerImporterItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, importerPK.ToGuid());
		}

		internal void SetValidRegistryMercedesImporter(ZGuid importerPK)
		{
			rego.MercedesImporterItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, importerPK.ToGuid());
		}

		internal void SetValidRegistryChryslerSupplier(ZGuid supplierPK)
		{
			rego.ChryslerSupplierItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, supplierPK.ToGuid());
		}

		internal void SetValidRegistryMercedesSupplier(ZGuid supplierPK)
		{
			rego.MercedesSupplierItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, supplierPK.ToGuid());
		}

#endregion
		static WCBDataRegistry rego
		{
			get
			{
				return WCBDataRegistry.Instance;
			}
		}
#endregion
	}
}
