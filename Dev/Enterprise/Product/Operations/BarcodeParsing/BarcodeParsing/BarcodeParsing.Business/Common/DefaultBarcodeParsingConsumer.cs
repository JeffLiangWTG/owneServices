using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;

using Enterprise.BarcodeParsingEngine;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.BarcodeParsing.Business
{
	public enum DummyTargetField
	{
		DummyElement
	}

	/// <summary>
	/// This consumer is only used by the BarcodeParsing Module when the user provides
	/// an empty or invalid module code via the GUI.
	/// </summary>
	public sealed class DefaultBarcodeParsingConsumer : BarcodeParsingConsumer<DummyTargetField>
	{
		internal DefaultBarcodeParsingConsumer(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		#region ModuleCode

		protected override ZString ModuleCode
		{
			get { return ZString.Empty; }
		}

		#endregion

		#region Buyers

		protected override OrgHeaderCollection Buyers
		{
			get { return new ConsigneeCollection(Factory); }
		}

		#endregion

		#region Suppliers

		protected override OrgHeaderCollection Suppliers
		{
			get { return new ConsignorCollection(Factory); }
		}

		#endregion

		#region GetRelatedEntityList

		protected override IBusinessObjectCollection GetRelatedEntityList(OrgHeader buyer, OrgHeader supplier)
		{
			return null;
		}

		#endregion

		#region TargetFields

		protected override ReadOnlyCodeDescriptionPairList TargetFields
		{
			get { return new ReadOnlyCodeDescriptionPairList(); }
		}

		#endregion

		#region GS1TargetFieldsToDefault

		protected override IEnumerable<ZString> GS1TargetFieldsToDefault
		{
			get { return Enumerable.Empty<ZString>(); }
		}

		#endregion

		#region IsRelatedEntityAvailable

		protected override bool IsRelatedEntityAvailable
		{
			get { return false; }
		}

		#endregion

		#region IsBuyerAvailable

		protected override bool IsBuyerAvailable
		{
			get { return false; }
		}

		#endregion

		#region IsSupplierAvailable

		protected override bool IsSupplierAvailable
		{
			get { return false; }
		}

		#endregion

		#region GetValidFieldFormatsForTargetField

		protected override IEnumerable<FormatType> GetValidFieldFormatsForTargetField(ZBool isGS1, ZString targetField)
		{
			return System.Array.Empty<FormatType>();
		}

		#endregion
	}
}
