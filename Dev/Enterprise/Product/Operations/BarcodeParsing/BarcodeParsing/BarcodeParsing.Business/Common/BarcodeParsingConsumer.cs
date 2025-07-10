using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BarcodeParsingEngine;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.BarcodeParsing.Business
{
	public abstract class BarcodeParsingConsumer<T> : IBarcodeParsingConsumer
		where T : struct, IFormattable, IConvertible, IComparable
	{
		protected BarcodeParsingConsumer(BusinessObjectFactory factory)
		{
			this.factory = CargoWise.Common.Argument.NotNull(factory, "factory");
		}

		readonly BusinessObjectFactory factory;

		#region Factory

		public BusinessObjectFactory Factory
		{
			get { return factory; }
		}

		#endregion

		#region BuyerCaption

		string IBarcodeParsingConsumer.BuyerCaption
		{
			get { return BuyerCaption; }
		}

		protected virtual string BuyerCaption
		{
			get { return null; }
		}

		#endregion

		#region RelatedEntityCaption

		string IBarcodeParsingConsumer.RelatedEntityCaption
		{
			get { return RelatedEntityCaption; }
		}

		protected virtual string RelatedEntityCaption
		{
			get { return null; }
		}

		#endregion

		#region SupplierCaption

		string IBarcodeParsingConsumer.SupplierCaption
		{
			get { return SupplierCaption; }
		}

		protected virtual string SupplierCaption
		{
			get { return null; }
		}

		#endregion

		#region ModuleCode

		ZString IBarcodeParsingConsumer.ModuleCode
		{
			get { return ModuleCode; }
		}

		protected abstract ZString ModuleCode { get; }

		#endregion

		#region Buyers

		OrgHeaderCollection IBarcodeParsingConsumer.Buyers
		{
			get { return Buyers; }
		}

		protected abstract OrgHeaderCollection Buyers { get; }

		#endregion

		#region Suppliers

		OrgHeaderCollection IBarcodeParsingConsumer.Suppliers
		{
			get { return Suppliers; }
		}

		protected abstract OrgHeaderCollection Suppliers { get; }

		#endregion

		#region GetRelatedEntityList

		IBusinessObjectCollection IBarcodeParsingConsumer.GetRelatedEntityList(OrgHeader buyer, OrgHeader supplier)
		{
			return GetRelatedEntityList(buyer, supplier);
		}

		protected abstract IBusinessObjectCollection GetRelatedEntityList(OrgHeader buyer, OrgHeader supplier);

		#endregion

		#region RelatedEntityRequirements

		RelatedEntityRequirements IBarcodeParsingConsumer.RelatedEntityRequirements
		{
			get { return RelatedEntityRequirements; }
		}

		protected virtual RelatedEntityRequirements RelatedEntityRequirements
		{
			get { return RelatedEntityRequirements.None; }
		}

		#endregion

		#region TargetFields

		ReadOnlyCodeDescriptionPairList IBarcodeParsingConsumer.TargetFields
		{
			get { return TargetFields; }
		}

		protected abstract ReadOnlyCodeDescriptionPairList TargetFields { get; }

		#endregion

		#region GS1TargetFieldsToDefault

		IEnumerable<ZString> IBarcodeParsingConsumer.GS1TargetFieldsToDefault
		{
			get { return GS1TargetFieldsToDefault; }
		}

		protected abstract IEnumerable<ZString> GS1TargetFieldsToDefault { get; }

		#endregion

		#region IsRelatedEntityAvailable

		bool IBarcodeParsingConsumer.IsRelatedEntityAvailable
		{
			get { return IsRelatedEntityAvailable; }
		}

		protected abstract bool IsRelatedEntityAvailable { get; }

		#endregion

		#region IsBuyerAvailable

		bool IBarcodeParsingConsumer.IsBuyerAvailable
		{
			get { return IsBuyerAvailable; }
		}

		protected abstract bool IsBuyerAvailable { get; }

		#endregion

		#region IsSupplierAvailable

		bool IBarcodeParsingConsumer.IsSupplierAvailable
		{
			get { return IsSupplierAvailable; }
		}

		protected abstract bool IsSupplierAvailable { get; }

		#endregion

		#region GetValidFieldFormatsForTargetField

		IEnumerable<FormatType> IBarcodeParsingConsumer.GetValidFieldFormatsForTargetField(ZBool isGS1, ZString targetField)
		{
			return GetValidFieldFormatsForTargetField(isGS1, targetField);
		}

		protected abstract IEnumerable<FormatType> GetValidFieldFormatsForTargetField(ZBool isGS1, ZString targetField);

		#endregion
	}
}
