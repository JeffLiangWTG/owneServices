using CargoWise.ComponentModel;
using Enterprise.DataTransfer.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.OrgPatternMatching;
using Enterprise.ZArchitecture;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.DataTransfer.DataAdapters
{
	public class StandardManualAndBatchImportAddressValueObjectHelper : AddressValueObjectHelper
	{
		public StandardManualAndBatchImportAddressValueObjectHelper(string errorContext)
			: base(errorContext)
		{
		}

		protected override bool GetIsMatchingAddress(IMatchingAddress currentAddress, Xsd.OrgAddress addressValue, IOrgHeaderForMatching organisation, IValueObjectImportContext context)
		{
			if (!addressValue.AddressCode.IsEmpty)
			{
				return addressValue.AddressCode.EqualsIgnoringCase(currentAddress.OA_Code);
			}
			else
			{
				var isMatchingAddressWithoutCode = base.GetIsMatchingAddress(currentAddress, addressValue, organisation, context);
				if (isMatchingAddressWithoutCode)
				{
					context.Notify(new WarningNotification(WarningType.Warning, Res.GetString("57a2ff98-c550-460d-b06e-6daee94ee813", "Address Code is empty on organization {0}, matched with existing address '{1}' using address lines.", organisation.OH_Code, currentAddress.OA_Code)));
				}
				return isMatchingAddressWithoutCode;
			}
		}

		protected override bool GetIsFuzzyMatching(IMatchingAddress currentAddress, Xsd.OrgAddress addressValue)
		{
			string xsdAddressCode = AddressLineFuzzyMatch.GetStringForFuzzyComparing(addressValue.AddressCode);
			string orgAddressCode = AddressLineFuzzyMatch.GetStringForFuzzyComparing(currentAddress.OA_Code);

			return xsdAddressCode.CompareTo(orgAddressCode) == 0;
		}
	}
}
