using System;

namespace Enterprise.Customs.Common
{
	public interface IHaveAdditionalDataForBorderWise
	{
		/// <summary>
		/// Type of Reference File Tariff Business Object eg. USCTariff
		/// </summary>
		Type ExpectedBusinessObjectTypeForList { get; }
		AdditionalDataForBorderWise GetAdditionalDataForBorderWise(string bindingProperty);
	}
}
