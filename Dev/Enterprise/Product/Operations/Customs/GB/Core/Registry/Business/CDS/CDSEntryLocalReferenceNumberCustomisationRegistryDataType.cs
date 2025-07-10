using System;
using Enterprise.Registry.Business;

namespace Enterprise.Customs.GB.Registry
{
	public class CDSEntryLocalReferenceNumberCustomisationRegistryDataType : BillCustomisationRegistryDataType
	{
		public CDSEntryLocalReferenceNumberCustomisationRegistryDataType() : base(new CDSEntryLocalReferenceNumberCustomisation())
		{
			GeneratedNumberName = ResString.GetMultilingualString("41354967-AD7C-4328-9FA0-44362E281008", "CDS Entry Local Reference Number");
			MaxLength = GBCustomsDataRegistry.CDSEntryLocalReferenceNumberMaxLength;
		}

		protected override Type DataTypeCore => typeof(CDSEntryLocalReferenceNumberCustomisation);
	}
}
