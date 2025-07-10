using System.Collections.Generic;
using System.Linq;

using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BarcodeParsingEngine;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.BarcodeParsing.Business.Testing
{
	public class DummyBarcodeParsingConsumer : IBarcodeParsingConsumer
	{
		public const string Module = "DUM";

		public DummyBarcodeParsingConsumer(BusinessObjectFactory factory)
		{
			Factory = factory;
		}

		public static DummyBarcodeParsingConsumer GetDummy(BusinessObjectFactory factory)
		{
			return (DummyBarcodeParsingConsumer)factory.GetBarcodeParsingConsumerFromModuleCode(Module);
		}

		public BusinessObjectFactory Factory
		{
			get;
			private set;
		}

		public string BuyerCaption
		{
			get { return "Dummy Buyer"; }
		}

		public string RelatedEntityCaption
		{
			get { return "Dummy Entity"; }
		}

		public string SupplierCaption
		{
			get { return "Dummy Supplier"; }
		}

		public ZString ModuleCode
		{
			get { return Module; }
		}

		public OrgHeaderCollection Buyers
		{
			get { return buyers ?? (buyers = new OrgHeaderCollection(Factory)); }
			set { buyers = value; }
		}

		OrgHeaderCollection buyers;

		public OrgHeaderCollection Suppliers
		{
			get { return suppliers ?? (suppliers = new OrgHeaderCollection(Factory)); }
			set { suppliers = value; }
		}

		OrgHeaderCollection suppliers;

		public IBusinessObjectCollection GetRelatedEntityList(OrgHeader buyer, OrgHeader supplier)
		{
			return RelatedEntityList;
		}

		public IBusinessObjectCollection RelatedEntityList
		{
			get;
			set;
		}

		public ReadOnlyCodeDescriptionPairList TargetFields
		{
			get { return new DummyTargetFields(); }
		}

		public IEnumerable<ZString> GS1TargetFieldsToDefault
		{
			get { return new ZString[] { DummyTargetFields.Codes.TargetField2, DummyTargetFields.Codes.TargetField3 }; }
		}

		public RelatedEntityRequirements RelatedEntityRequirements
		{
			get;
			set;
		}

		public bool IsRelatedEntityAvailable
		{
			get;
			set;
		}

		public bool IsBuyerAvailable
		{
			get;
			set;
		}

		public bool IsSupplierAvailable
		{
			get;
			set;
		}

		public IEnumerable<FormatType> GetValidFieldFormatsForTargetField(ZBool isGS1, ZString targetField)
		{
			var result = Enumerable.Empty<FormatType>();
			if (ValidFormatTypesByField.ContainsKey(targetField))
			{
				result = ValidFormatTypesByField[targetField];
			}
			return result;
		}

		readonly Dictionary<ZString, IEnumerable<FormatType>> ValidFormatTypesByField = new Dictionary<ZString, IEnumerable<FormatType>>();
		public void SetFormatsForTargetField(ZString targetField, IEnumerable<FormatType> formatTypes)
		{
			ValidFormatTypesByField[targetField] = formatTypes;
		}
	}
}
