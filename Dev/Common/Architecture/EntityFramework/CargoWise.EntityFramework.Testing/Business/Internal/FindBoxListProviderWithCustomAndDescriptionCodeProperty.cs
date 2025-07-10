using System;
using CargoWise.Types;

namespace CargoWise.EntityFramework.Testing
{
	internal class FindBoxListProviderWithCustomAndDescriptionCodeProperty : FindBoxListProvider
	{
		public string CodePropertyName { get; set; } = "Code";
		public string DescriptionPropertyName { get; set; } = "Description";

		public FindBoxListProviderWithCustomAndDescriptionCodeProperty(IBusinessObjectCollection collection) : base(collection)
		{
		}

		protected override string GetCodePropertyName(ZGuid pK)
			=> CodePropertyName;

		protected override string GetCodePropertyName(Type typeOfElement)
			=> CodePropertyName;

		protected override string GetDescriptionPropertyName(Type typeOfElements)
			=> DescriptionPropertyName;
	}
}
