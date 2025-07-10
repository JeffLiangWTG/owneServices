using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BarcodeParsingEngine;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.BarcodeParsing.Business
{
	public interface IBarcodeParsingConsumer
	{
		BusinessObjectFactory Factory { get; }

		// Captions
		string BuyerCaption { get; }
		string RelatedEntityCaption { get; }
		string SupplierCaption { get; }

		// Module
		ZString ModuleCode { get; }

		// Lists
		OrgHeaderCollection Buyers { get; }
		OrgHeaderCollection Suppliers { get; }
		IBusinessObjectCollection GetRelatedEntityList(OrgHeader buyer, OrgHeader supplier);
		ReadOnlyCodeDescriptionPairList TargetFields { get; }
		IEnumerable<ZString> GS1TargetFieldsToDefault { get; }

		// Related Entity
		RelatedEntityRequirements RelatedEntityRequirements { get; }
		bool IsRelatedEntityAvailable { get; }

		//Buyer and Supplier
		bool IsBuyerAvailable { get; }
		bool IsSupplierAvailable { get; }

		// Validation
		IEnumerable<FormatType> GetValidFieldFormatsForTargetField(ZBool isGS1, ZString targetField);
	}
}
