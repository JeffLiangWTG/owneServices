using System;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.EU.Registry
{
	[RegistryEditor("Enterprise.Registry.GUI.BillOfLadingNumberCustomisationRegistryItemEditor, Enterprise.Registry.GUI")]
	public sealed class LimitedFiscalRepresentationNumberCustomisationRegistryDataType : BillCustomisationRegistryDataType
	{
		public LimitedFiscalRepresentationNumberCustomisationRegistryDataType()
			: base(new LimitedFiscalRepresentationNumberCustomisation())
		{
			GeneratedNumberName = Business.ResString.GetMultilingualString("E8824A2A-C199-49D9-8E5A-291AF4BB3EB2", "LFR Number");
			MaxLength = CusReferenceSchema.CFR_Reference.MaxLength;
			PrefixLength = 3;
		}

		protected override Type DataTypeCore => typeof(LimitedFiscalRepresentationNumberCustomisation);
	}
}
