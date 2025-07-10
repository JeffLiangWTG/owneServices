using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.PBN.Business.Testing
{
	[TestedType(typeof(PBNCustomsDeclarationItemCollection<PBNCustomsDeclarationItem>))]
	class PBNCustomsDeclarationItemCollectionTest : CusSupportingInfoCollectionTest<PBNCustomsDeclarationItem>
	{
		protected override CusSupportingInfoCollection<PBNCustomsDeclarationItem> GetCusSupportingInfoCollection()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			return new PBNCustomsDeclarationItemCollection<PBNCustomsDeclarationItem>(header, CustomsReferenceCodes);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var result = base.GetNewElementToAddToTheCollection();
			((PBNCustomsDeclarationItem)result).CSI_Code = "AIS";
			return result;
		}

		static IEnumerable<string> CustomsReferenceCodes
		{
			get
			{
				yield return IEPBNDeclarationTypes.Codes.AIS;
				yield return IEPBNDeclarationTypes.Codes.AES;
				yield return IEPBNDeclarationTypes.Codes.ICS;
			}
		}
	}
}
