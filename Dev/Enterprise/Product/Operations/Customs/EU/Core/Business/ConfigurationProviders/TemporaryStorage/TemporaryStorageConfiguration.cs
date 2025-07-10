using System;
using System.Collections;
using CargoWise.Application;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.EU.Business.CusTempStorage
{
	public class TemporaryStorageConfiguration
	{
		public static TemporaryStorageConfiguration GetConfiguration(BusinessObjectFactory factory, string countryOrGrouping)
		{
			return factory.GetCachedValue(FormattableString.Invariant($"TemporaryStorageConfiguration_{countryOrGrouping}"), () =>
			{
				object supporter = null;
				var builders = ObjectFactory.Get<Hashtable>("TemporaryStorageConfiguration");
				if (!string.IsNullOrEmpty(countryOrGrouping))
				{
					var objectHandle = (ObjectHandle)builders[countryOrGrouping];
					supporter = objectHandle?.GetObject();
				}
				if (supporter == null)
				{
					var objectHandle = (ObjectHandle)builders[Core.Constants.CountryCodes.EuropeanUnion];
					supporter = objectHandle.GetObject();
				}
				return (TemporaryStorageConfiguration)supporter;
			});
		}

		public TemporaryStoragePreviousDocumentConfiguration PreviousDocumentConfiguration => previousDocumentConfiguration ?? (previousDocumentConfiguration = GetNewPreviousDocumentConfiguration());
		TemporaryStoragePreviousDocumentConfiguration previousDocumentConfiguration;
		protected virtual TemporaryStoragePreviousDocumentConfiguration GetNewPreviousDocumentConfiguration() => new TemporaryStoragePreviousDocumentConfiguration();

		public TemporaryStorageSupportingDocumentConfiguration SupportingDocumentConfiguration => supportingDocumentConfiguration ?? (supportingDocumentConfiguration = GetNewSupportingDocumentConfiguration());
		TemporaryStorageSupportingDocumentConfiguration supportingDocumentConfiguration;
		protected virtual TemporaryStorageSupportingDocumentConfiguration GetNewSupportingDocumentConfiguration() => new TemporaryStorageSupportingDocumentConfiguration();

		public TemporaryStorageBillConfiguration BillConfiguration => billConfiguration ??= GetNewBillConfiguration();
		TemporaryStorageBillConfiguration billConfiguration;
		protected virtual TemporaryStorageBillConfiguration GetNewBillConfiguration() => new TemporaryStorageBillConfiguration();

		public ITemporaryStorageHeaderValidationDecider GetValidationDecider() => GetValidationDeciderCore();
		protected virtual ITemporaryStorageHeaderValidationDecider GetValidationDeciderCore() => new TemporaryStorageHeaderValidationDecider();

		public TemporaryStoragePackedItemConfiguration PackedItemConfiguration => packedItemConfiguration ??= GetNewPackedItemConfiguration();
		TemporaryStoragePackedItemConfiguration packedItemConfiguration;
		protected virtual TemporaryStoragePackedItemConfiguration GetNewPackedItemConfiguration() => new TemporaryStoragePackedItemConfiguration();

		public bool SupportLRNGeneration => SupportLRNGenerationCore;
		protected virtual bool SupportLRNGenerationCore => true;

		public bool SupportAgentDefaulting => SupportAgentDefaultingCore;
		protected virtual bool SupportAgentDefaultingCore => true;

		public string TemporaryStorageHeaderDocumentWrapperClass => TemporaryStorageHeaderDocumentWrapperClassCore;
		protected virtual string TemporaryStorageHeaderDocumentWrapperClassCore => "Enterprise.DocumentWrappers.Customs.EU.TemporaryStorage.TemporaryStorageHeaderWrapper";
	}
}
