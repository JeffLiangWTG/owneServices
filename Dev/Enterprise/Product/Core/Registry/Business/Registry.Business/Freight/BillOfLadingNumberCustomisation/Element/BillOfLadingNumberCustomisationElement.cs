using System;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business.BillCustomisationStrategies;
using Enterprise.ZArchitecture;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	[System.Diagnostics.DebuggerDisplay("Key = {Key}")]
	public class BillOfLadingNumberCustomisationElement : AutoBillOfLadingNumberCustomisationElement, IXmlSerializable, ICalcMaxGeneratedLength
	{
		#region Keys

		public static class Keys
		{
			// WARNING: changing or removing an existing key will require a transform!
			// (but it's still safe to add a new key without a transform).
			// 
			// Also, if you change a code such that sorting by the key will result in a different order,
			// then you will also need to add a transform that updates the generated number fountain names.
			// Since this will be very dificualt (possibly even impossible) to do it reliably, you should
			// avoid any such changes. (these keys are not user visible anyway).
			public const string BranchCode = "BranchCode";
			public const string CarrierPrincipalCode = "CarrierPrincipalCode";
			public const string ClientCoded1 = "ClientCoded";
			public const string ClientCoded2 = "ClientCoded2";
			public const string ClientCoded3 = "ClientCoded3";
			public const string CompanyCode = "CompanyCode";
			public const string ContainerTranshipmentIndicator = "ContainerTranshipmentIndicator";
			public const string ClientOrganisation = "ClientOrganisation";
			public const string DestinationIATA = "DestinationIATA";
			public const string DestinationUNLOCO = "DestinationUNLOCO";
			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "May be an identifier or GUID.")]
			public const string Direction = "Direction";
			public const string DischargeIATA = "DischargeIATA";
			public const string DischargeUNLOCO = "DischargeUNLOCO";
			public const string EnterpriseCode = "EnterpriseCode";
			public const string FirstLoadIATA = "FirstLoadIATA";
			public const string FirstLoadUNLOCO = "FirstLoadUNLOCO";
			public const string GlobalOrLocal = "GlobalOrLocal";
			public const string JobNo = "JobNo";
			public const string LastDischargeIATA = "LastDischargeIATA";
			public const string LastDischargeUNLOCO = "LastDischargeUNLOCO";
			public const string LoadIATA = "LoadIATA";
			public const string LoadUNLOCO = "LoadUNLOCO";
			public const string MonthAs2Digits = "MonthAs2Digits";
			public const string MonthAsLetter = "MonthAsLetter";
			public const string OriginIATA = "OriginIATA";
			public const string OriginUNLOCO = "OriginUNLOCO";
			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "May be an identifier or GUID.")]
			public const string Quarter = "Quarter";
			public const string SequenceNumber = "SequenceNumber";
			public const string ServerCode = "ServerCode";
			public const string ServiceLevel = "ServiceLevel";
			public const string SundryChargesActivity = "SundryChargesActivity";
			public const string SundryChargesMode = "SundryChargesMode";
			public const string SundryChargesType = "SundryChargesType";
			public const string TransportMode = "TransportMode";
			public const string UniversalOfficeCode = "UniversalOfficeCode";
			public const string WarehouseCode = "WarehouseCode";
			public const string WarehouseClientCode = "WarehouseClientCode";
			public const string WarehouseSalesChannelCode = "WarehouseSalesChannelCode";
			public const string WarehouseSubType = "WarehouseSubType";
			public const string WarehouseSupplierCode = "WarehouseSupplierCode";
			public const string WarehouseReceiveCategoryCode = "WarehouseReceiveCategoryCode";
			public const string YearAsDigit = "YearAsDigit";
			public const string YearAsLetter = "YearAsLetter";
		}

		#endregion

		#region XmlConstants

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "May be an identifier or GUID.")]
		public static class XmlConstants
		{
			public const string Elements = "Elements";
			public const string Element = "Element";
			public const string Order = "Order";
			public const string Fountain = "Fountain";
			public const string CheckDigit = "CheckDigit";
			public const string Detail = "Detail";

			public const string key = "key";
		}

		#endregion

		protected internal BillOfLadingNumberCustomisationElement(BillOfLadingNumberCustomisation parentCustomisation, IElementStrategy strategy)
		{
			if (parentCustomisation == null)
			{
				throw new ArgumentNullException(nameof(parentCustomisation));
			}

			if (strategy == null)
			{
				throw new ArgumentNullException(nameof(strategy));
			}

			this.parentCustomisation = parentCustomisation;
			this.strategy = strategy;

			using (GetValidationSuspender())
			using (SuspendSettingHasChanges())
			{
				this.Detail = Strategy.DefaultDetail;
				this.Order = Strategy.DefaultOrder;
			}
		}

		public bool Matches(NumberCustomisationElementCategories categories)
		{
			return (Strategy.Categories & categories) > 0;
		}

		#region CopyValuesFrom

		public void CopyValuesFrom(BillOfLadingNumberCustomisationElement source)
		{
			Include = source.Include;
			Order = source.Order;
			Detail = source.Detail;
			Fountain = source.Fountain;
			CheckDigit = source.CheckDigit;
		}

		#endregion

		#region Bound Properties

		#region Key

		public override ZString Key
		{
			get { return Strategy.Key; }
		}

		#endregion

		#region ElementName

		public override ZString ElementName
		{
			get { return Strategy.Name; }
		}

		#endregion

		#region Description

		public override ZString Description
		{
			get { return Strategy.Description; }
		}

		#endregion

		#region Detail

		public override ZString Detail
		{
			get => base.Detail;
			set
			{
				base.Detail = value;

				if (DetailIsMacroForCustomElement)
				{
					Fountain = false;
				}
			}
		}

		bool DetailIsMacroForCustomElement
		{
			get
			{
				var isCustomElement = Key.EqualsIgnoringCase(Keys.ClientCoded1)
					|| Key.EqualsIgnoringCase(Keys.ClientCoded2)
					|| Key.EqualsIgnoringCase(Keys.ClientCoded3);
				return isCustomElement && Detail.IsMacro();
			}
		}

		protected override bool Detail_ReadOnly
		{
			get { return !Strategy.UseDetail || !Include; }
		}

		protected override int Detail_MaxLength
		{
			get
			{
				int result = Strategy == null ? -1 : Strategy.DetailMaxLength;
				return result > 0 ? result : base.Detail_MaxLength;
			}
		}

		#endregion

		#region DetailType

		public override ZString DetailType
		{
			get { return Strategy.DetailFieldType.ToString(); }
		}

		#endregion

		#region DecimalPlacesForBinding

		public ZInt DecimalPlacesForBinding
		{
			get
			{
				int result = 2;
				switch (Strategy.DetailFieldType)
				{
					case FieldType.Decimal:
						result = 6;
						break;

					case FieldType.Byte:
					case FieldType.Integer:
						result = 0;
						break;
				}
				return result;
			}
		}

		#endregion

		#region Order

		public override ZByte Order
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return base.Order; }
			set
			{
				int oldOrder = base.Order;
				int newOrder = value;

				base.Order = value;

				if (!IsValidationSuspended && oldOrder != newOrder)
				{
					foreach (BillOfLadingNumberCustomisationElement element in ParentCustomisation.Elements)
					{
						if (element != this && (element.Order == oldOrder || element.Order == newOrder))
						{
							element.Validation.ValidateOrder();
						}
					}
				}
			}
		}

		protected override bool Order_ReadOnly
		{
			get { return !Include || Strategy.ReadOnly || Strategy.OrderReadOnly; }
		}

		#endregion

		#region Include

		public override ZBool Include
		{
			get { return base.Include || Strategy.Force; }
			set
			{
				if (Include != value)
				{
					base.Include = value;
					Order = value ? FindNextOrderNumber() : (byte)0;
					Detail = Strategy.DefaultDetail;
				}
			}
		}

		protected override bool Include_ReadOnly
		{
			get { return Strategy.Force || Strategy.ReadOnly || Strategy.IncludeReadOnly; }
		}

		#endregion

		#region Fountain

		public override ZBool Fountain
		{
			get { return base.Fountain && !Strategy.Force; }
			[System.Diagnostics.DebuggerStepThrough]
			set { base.Fountain = value; }
		}

		protected override bool Fountain_ReadOnly
		{
			get
			{
				return Strategy.Force || !Include || Strategy.ReadOnly || DetailIsMacroForCustomElement;
			}
		}

		#endregion

		#region CheckDigit

		protected override bool CheckDigit_ReadOnly
		{
			get { return !Include || Strategy.ReadOnly; }
		}

		#endregion

		#endregion

		#region Strategy

		internal IElementStrategy Strategy
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return strategy; }
		}

		#endregion

		#region ParentCustomisation

		public BillOfLadingNumberCustomisation ParentCustomisation
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return parentCustomisation; }
		}

		#endregion

		#region BusinessObject Overrides

		public override void Delete()
		{
			throw new CannotDeleteException("If you do not want this element to appear in the generated output then simply un-tick include.");
		}

		protected override BillOfLadingNumberCustomisationElementValidation GetNewValidation()
		{
			return Strategy.GetValidation(this);
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			CheckDigit = true;
		}

		#endregion

		#region Implementation

		byte FindNextOrderNumber()
		{
			int nextOrder = 1;
			foreach (BusinessObjectCollection collection in ParentCollections)
			{
				foreach (BillOfLadingNumberCustomisationElement element in collection)
				{
					if (element != this && element.Order >= nextOrder && !element.Strategy.Force)
					{
						nextOrder = element.Order + 1;
					}
				}
			}
			return (byte)nextOrder;
		}

		public override bool Equals(object obj)
		{
			//Derived from the 'CopyValuesFrom' method
			var other = obj as BillOfLadingNumberCustomisationElement;
			return other != null &&
				Key == other.Key &&
				ElementName == other.ElementName &&
				Include == other.Include &&
				Order == other.Order &&
				Detail == other.Detail &&
				Fountain == other.Fountain &&
				CheckDigit == other.CheckDigit;
		}

		public override int GetHashCode()
		{
			return Key.GetHashCode();
		}

		#endregion

		#region IXmlSerializable Members

		XmlSchema IXmlSerializable.GetSchema()
		{
			return null;
		}

		public void ReadXml(XmlReader reader)
		{
			reader.ReadStartElement(XmlConstants.Element);
			Include = true;
			Order = ZByte.ParseSafe(reader.ReadElementString(XmlConstants.Order), 0);

			if (reader.Name == XmlConstants.Fountain)
			{
				Fountain = new ZBool(reader.ReadElementString(XmlConstants.Fountain));
			}
			else
			{
				Fountain = false;
			}

			if (reader.Name == XmlConstants.CheckDigit)
			{
				CheckDigit = new ZBool(reader.ReadElementString(XmlConstants.CheckDigit));
			}

			if (reader.Name == XmlConstants.Detail)
			{
				Detail = reader.ReadElementString(XmlConstants.Detail);
			}
			else
			{
				Detail = Strategy.DefaultDetail;
			}

			reader.ReadEndElement();
		}

		public void WriteXml(XmlWriter writer)
		{
			writer.WriteStartElement(XmlConstants.Element);
			writer.WriteAttributeString(XmlConstants.key, Key);
			writer.WriteElementString(XmlConstants.Order, Order.ToString());

			if (Fountain)
			{
				writer.WriteElementString(XmlConstants.Fountain, Fountain.ToString());
			}

			writer.WriteElementString(XmlConstants.CheckDigit, CheckDigit.ToString());

			if (!Detail.IsEmpty)
			{
				writer.WriteElementString(XmlConstants.Detail, Detail);
			}

			writer.WriteEndElement();
		}

		#endregion

		int ICalcMaxGeneratedLength.CalcMaxGeneratedLength => Strategy.CalcMaxGeneratedLength(this);

		public string RegExForDataType => Strategy.GetRegExForDataType(this);

		public void OverrideStrategy(IElementStrategy strategy)
		{
			this.strategy = strategy;
		}

		[System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
		IElementStrategy strategy;
		[System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
		readonly BillOfLadingNumberCustomisation parentCustomisation;
	}
}
