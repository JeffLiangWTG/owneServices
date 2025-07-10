using System;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.CA.Registry;

namespace Enterprise.Customs.CA.Business
{
	public class PackingSynchroniser : Customs.Business.PackingSynchroniser
	{
		public PackingSynchroniser(JobDeclarationSynchroniser parentSynchroniser, BaseJobDeclaration declaration)
			: base(parentSynchroniser, declaration)
		{
		}

		protected override ZString GetConvertedPackUQ(ZString freightPackageType)
		{
			var result = ZString.Empty;
			var declaration = Declaration as JobDeclaration;
			if (declaration != null && declaration.IsIID)
			{
				var mappings = CACustomsDataRegistry.Instance.CAPackageTypesMapping.GetValueWithoutFallback(declaration.RegistryCompanyPK, Guid.Empty, Guid.Empty);
				result = mappings.GetMappedPackageType(freightPackageType);
			}
			if (result.IsEmpty)
			{
				result = freightPackageType;
			}
			return result;
		}

		protected override ZString GetConvertedMarksAndNumbers(ZString marksAndNumbers)
		{
			var declaration = Declaration as JobDeclaration;
			return declaration != null && declaration.IsIID ? marksAndNumbers.Left(35) : marksAndNumbers;
		}
	}
}
