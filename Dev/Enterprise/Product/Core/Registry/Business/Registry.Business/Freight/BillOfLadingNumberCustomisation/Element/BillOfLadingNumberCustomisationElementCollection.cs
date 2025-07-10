using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business.BillCustomisationStrategies;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Registry.Business
{
	public partial class BillOfLadingNumberCustomisationElementCollection : NonPersistentBusinessObjectCollection<BillOfLadingNumberCustomisationElement>, IXmlSerializable
	{
		public BillOfLadingNumberCustomisationElementCollection(BillOfLadingNumberCustomisation parentCustomisation)
		{
			this.parentCustomisation = parentCustomisation;
		}

		public BillOfLadingNumberCustomisationElement this[string key]
		{
			get
			{
				foreach (BillOfLadingNumberCustomisationElement element in this)
				{
					if (element.Key == key)
					{
						return element;
					}
				}
				return null;
			}
		}

		public BillOfLadingNumberCustomisation ParentCustomisation
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return parentCustomisation; }
		}

		#region NewAndPopulate

		public static BillOfLadingNumberCustomisationElementCollection NewAndPopulate(BillOfLadingNumberCustomisation parentCustomisation)
		{
			BillOfLadingNumberCustomisationElementCollection result = new BillOfLadingNumberCustomisationElementCollection(parentCustomisation);
			result.AddNew(BillOfLadingNumberCustomisationElement.Keys.BranchCode, (NoResString)"Branch Code", "", 3
				, NumberCustomisationElementCategories.Standard
				, StrategyRegExFormattingHelper.GetExactlyLengthRegExValue);
			result.AddNew(BillOfLadingNumberCustomisationElement.Keys.CarrierPrincipalCode, (NoResString)"Carrier/Principal Code", string.Empty, 3
				, NumberCustomisationElementCategories.LinerAgency
				, StrategyRegExFormattingHelper.GetExactlyLengthRegExValue);
			result.AddNew(new ClientCodedElementStrategy(BillOfLadingNumberCustomisationElement.Keys.ClientCoded1));
			result.AddNew(new ClientCodedElementStrategy(BillOfLadingNumberCustomisationElement.Keys.ClientCoded2));
			result.AddNew(new ClientCodedElementStrategy(BillOfLadingNumberCustomisationElement.Keys.ClientCoded3));
			result.AddNew(BillOfLadingNumberCustomisationElement.Keys.CompanyCode, (NoResString)"Company Code", "", 3
				, NumberCustomisationElementCategories.Standard | NumberCustomisationElementCategories.ClientContract
				, StrategyRegExFormattingHelper.GetExactlyLengthRegExValue);
			result.AddNew(BillOfLadingNumberCustomisationElement.Keys.ContainerTranshipmentIndicator, (NoResString)"Container Transhipment Indicator", string.Empty, 1
				, NumberCustomisationElementCategories.LinerAgency
				, StrategyRegExFormattingHelper.GetExactlyLengthRegExValue);
			result.AddNew(BillOfLadingNumberCustomisationElement.Keys.DestinationIATA, (NoResString)"Destination Port IATA", string.Empty, RefUNLOCOSchema.RL_IATA.MaxLength
				, NumberCustomisationElementCategories.FreightStandard
				, StrategyRegExFormattingHelper.GetMaxToLengthRegExValue);
			result.AddNew(BillOfLadingNumberCustomisationElement.Keys.DestinationUNLOCO, (NoResString)"Destination Port UNLOCO", string.Empty, RefUNLOCOSchema.RL_Code.MaxLength
				, NumberCustomisationElementCategories.FreightStandard
				, StrategyRegExFormattingHelper.GetExactlyLengthRegExValue);
			result.AddNew(BillOfLadingNumberCustomisationElement.Keys.Direction, (NoResString)"Direction", (NoResString)"Import = I, Export = E, Domestic = D, Other = O", 1
				, NumberCustomisationElementCategories.FreightStandard | NumberCustomisationElementCategories.Consol | NumberCustomisationElementCategories.EMCSDeclaration | NumberCustomisationElementCategories.ConsolidatedDeclaration
				, (_) => "[IEDO]{1}");
			result.AddNew(BillOfLadingNumberCustomisationElement.Keys.EnterpriseCode, (NoResString)"Enterprise Code", string.Empty, 3
				, NumberCustomisationElementCategories.Standard
				, StrategyRegExFormattingHelper.GetExactlyLengthRegExValue);
			result.AddNew(BillOfLadingNumberCustomisationElement.Keys.JobNo, (NoResString)"Job Number", (NoResString)"Parent Job Number, falls back to Package Job Number", 20
				, NumberCustomisationElementCategories.PackageID
				, StrategyRegExFormattingHelper.GetMaxToLengthRegExValue);
			result.AddNew(new MonthAs2DigitsElementStrategy());
			result.AddNew(BillOfLadingNumberCustomisationElement.Keys.MonthAsLetter, (NoResString)"Month as Letter", (NoResString)"Jan = A, Feb = B ... Dec = L", 1
				, NumberCustomisationElementCategories.Standard
				, (_) => "[A-L]{1}");
			result.AddNew(BillOfLadingNumberCustomisationElement.Keys.OriginIATA, (NoResString)"Origin Port IATA", string.Empty, RefUNLOCOSchema.RL_IATA.MaxLength
				, NumberCustomisationElementCategories.FreightStandard
				, StrategyRegExFormattingHelper.GetMaxToLengthRegExValue);
			result.AddNew(BillOfLadingNumberCustomisationElement.Keys.OriginUNLOCO, (NoResString)"Origin Port UNLOCO", string.Empty, RefUNLOCOSchema.RL_Code.MaxLength
				, NumberCustomisationElementCategories.FreightStandard
				, StrategyRegExFormattingHelper.GetExactlyLengthRegExValue);
			result.AddNew(new SequenceElementStrategy());
			result.AddNew(BillOfLadingNumberCustomisationElement.Keys.ServerCode, (NoResString)"Server Code", string.Empty, 3
				, NumberCustomisationElementCategories.Standard
				, StrategyRegExFormattingHelper.GetExactlyLengthRegExValue);
			result.AddNew(BillOfLadingNumberCustomisationElement.Keys.SundryChargesActivity, (NoResString)"Activity", string.Empty, 3
				, NumberCustomisationElementCategories.SundryCharges
				, StrategyRegExFormattingHelper.GetMaxToLengthRegExValue);
			result.AddNew(BillOfLadingNumberCustomisationElement.Keys.SundryChargesMode, (NoResString)"Mode", string.Empty, 3
				, NumberCustomisationElementCategories.SundryCharges
				, StrategyRegExFormattingHelper.GetMaxToLengthRegExValue);
			result.AddNew(BillOfLadingNumberCustomisationElement.Keys.SundryChargesType, (NoResString)"Type", string.Empty, 3
				, NumberCustomisationElementCategories.SundryCharges
				, StrategyRegExFormattingHelper.GetMaxToLengthRegExValue);
			result.AddNew(BillOfLadingNumberCustomisationElement.Keys.TransportMode, (NoResString)"Transport Mode", (NoResString)"Air = A, Sea = S, Rail = R, Road = O", 1
				, NumberCustomisationElementCategories.FreightStandard | NumberCustomisationElementCategories.Consol | NumberCustomisationElementCategories.EMCSDeclaration | NumberCustomisationElementCategories.ConsolidatedDeclaration
				, (_) => "[ASRO]{1}");
			result.AddNew(BillOfLadingNumberCustomisationElement.Keys.UniversalOfficeCode, (NoResString)"Universal Office Code", string.Empty, GetMaxUniversalOfficeCodeLength
				, NumberCustomisationElementCategories.Standard
				, StrategyRegExFormattingHelper.GetMaxToLengthRegExValue);
			result.AddNew(new YearAsDigitElementStrategy());
			result.AddNew(BillOfLadingNumberCustomisationElement.Keys.YearAsLetter, (NoResString)"Year as Letter", (NoResString)"2001 = A, 2002 = B ... 2026 = Z", 1
				, NumberCustomisationElementCategories.Standard | NumberCustomisationElementCategories.ClientContract
				, (_) => "[A-Z]{1}");
			result.AddNew(BillOfLadingNumberCustomisationElement.Keys.LoadIATA, (NoResString)"Load Port IATA", string.Empty, RefUNLOCOSchema.RL_IATA.MaxLength
				, NumberCustomisationElementCategories.LinerAgency
				, StrategyRegExFormattingHelper.GetMaxToLengthRegExValue);
			result.AddNew(BillOfLadingNumberCustomisationElement.Keys.LoadUNLOCO, (NoResString)"Load Port UNLOCO", string.Empty, RefUNLOCOSchema.RL_Code.MaxLength
				, NumberCustomisationElementCategories.LinerAgency
				, StrategyRegExFormattingHelper.GetExactlyLengthRegExValue);
			result.AddNew(BillOfLadingNumberCustomisationElement.Keys.FirstLoadIATA, (NoResString)"First Load Port IATA", string.Empty, RefUNLOCOSchema.RL_IATA.MaxLength
				, NumberCustomisationElementCategories.Consol
				, StrategyRegExFormattingHelper.GetMaxToLengthRegExValue);
			result.AddNew(BillOfLadingNumberCustomisationElement.Keys.FirstLoadUNLOCO, (NoResString)"First Load Port UNLOCO", string.Empty, RefUNLOCOSchema.RL_Code.MaxLength
				, NumberCustomisationElementCategories.Consol
				, StrategyRegExFormattingHelper.GetExactlyLengthRegExValue);
			result.AddNew(BillOfLadingNumberCustomisationElement.Keys.DischargeIATA, (NoResString)"Discharge Port IATA", string.Empty, RefUNLOCOSchema.RL_IATA.MaxLength
				, NumberCustomisationElementCategories.LinerAgency
				, StrategyRegExFormattingHelper.GetMaxToLengthRegExValue);
			result.AddNew(BillOfLadingNumberCustomisationElement.Keys.DischargeUNLOCO, (NoResString)"Discharge Port UNLOCO", string.Empty, RefUNLOCOSchema.RL_Code.MaxLength
				, NumberCustomisationElementCategories.LinerAgency
				, StrategyRegExFormattingHelper.GetExactlyLengthRegExValue);
			result.AddNew(BillOfLadingNumberCustomisationElement.Keys.LastDischargeIATA, (NoResString)"Last Discharge Port IATA", string.Empty, RefUNLOCOSchema.RL_IATA.MaxLength
				, NumberCustomisationElementCategories.Consol
				, StrategyRegExFormattingHelper.GetMaxToLengthRegExValue);
			result.AddNew(BillOfLadingNumberCustomisationElement.Keys.LastDischargeUNLOCO, (NoResString)"Last Discharge Port UNLOCO", string.Empty, RefUNLOCOSchema.RL_Code.MaxLength
				, NumberCustomisationElementCategories.Consol
				, StrategyRegExFormattingHelper.GetExactlyLengthRegExValue);
			result.AddNew(BillOfLadingNumberCustomisationElement.Keys.ServiceLevel, (NoResString)"Service Level", string.Empty, RefServiceLevelSchema.RS_Code.MaxLength
				, NumberCustomisationElementCategories.FreightStandard | NumberCustomisationElementCategories.Consol | NumberCustomisationElementCategories.Domestic | NumberCustomisationElementCategories.EMCSDeclaration | NumberCustomisationElementCategories.ConsolidatedDeclaration
				, StrategyRegExFormattingHelper.GetMaxToLengthRegExValue);
			result.AddNew(BillOfLadingNumberCustomisationElement.Keys.Quarter, (NoResString)"Quarter", (NoResString)"Q{quarter} where quarter -> 1=Jan-Mar, 2=Apr-Jun, 3=Jul-Sep,4=Oct-Dec, ", 2
				, NumberCustomisationElementCategories.Standard
				, (_) => "[1-4]{1}");
			result.AddNew(BillOfLadingNumberCustomisationElement.Keys.ClientOrganisation, (NoResString)"Client Organisation", string.Empty, OrgHeaderSchema.OH_Code.MaxLength
				, NumberCustomisationElementCategories.ClientContract
				, StrategyRegExFormattingHelper.GetMaxToLengthRegExValue);
			result.AddNew(new GlobalOrLocalElementStrategy());
			result.AddNew(BillOfLadingNumberCustomisationElement.Keys.WarehouseSubType, (NoResString)"Sub Type", string.Empty, WhsDocketSchema.WD_DocketSubType.MaxLength
				, NumberCustomisationElementCategories.WarehouseJob
				, StrategyRegExFormattingHelper.GetMaxToLengthRegExValue);
			result.AddNew(new VariableLengthElementStrategy(BillOfLadingNumberCustomisationElement.Keys.WarehouseCode, (NoResString)"Warehouse Code", WhsWarehouseSchema.WW_WarehouseCode.MaxLength, NumberCustomisationElementCategories.WarehouseJob));
			result.AddNew(new VariableLengthElementStrategy(BillOfLadingNumberCustomisationElement.Keys.WarehouseClientCode, (NoResString)"Client Code", OrgHeaderSchema.OH_Code.MaxLength, NumberCustomisationElementCategories.WarehouseJob));
			result.AddNew(new VariableLengthElementStrategy(BillOfLadingNumberCustomisationElement.Keys.WarehouseSalesChannelCode, (NoResString)"Sales Channel Code", WhsSalesChannelSchema.WSH_Code.MaxLength, NumberCustomisationElementCategories.WarehouseOrder));
			result.AddNew(new VariableLengthElementStrategy(BillOfLadingNumberCustomisationElement.Keys.WarehouseSupplierCode, (NoResString)"Supplier Code", OrgHeaderSchema.OH_Code.MaxLength, NumberCustomisationElementCategories.WarehouseReceive));
			result.AddNew(new VariableLengthElementStrategy(BillOfLadingNumberCustomisationElement.Keys.WarehouseReceiveCategoryCode, (NoResString)"Receive Category Code", WhsDocketSchema.WD_ReceiveCategory.MaxLength, NumberCustomisationElementCategories.WarehouseReceive));
			return result;
		}

		#endregion

		#region AddNew

		BillOfLadingNumberCustomisationElement AddNew(string key, string elementName, string description, int maxValueLength
			, NumberCustomisationElementCategories categories
			, Func<BillOfLadingNumberCustomisationElement, string> overrideRegExForDataType)
		{
			return AddNew(new CommonElementStrategy(key, elementName, description, categories, maxValueLength, overrideRegExForDataType));
		}

		BillOfLadingNumberCustomisationElement AddNew(string key, string elementName, string description, Func<int> getMaxValueLengthFunc
			, NumberCustomisationElementCategories categories
			, Func<BillOfLadingNumberCustomisationElement, string> overrideRegExForDataType)
		{
			return AddNew(new CommonElementStrategy(key, elementName, description, categories, getMaxValueLengthFunc, overrideRegExForDataType));
		}

		BillOfLadingNumberCustomisationElement AddNew(IElementStrategy strategy)
		{
			BillOfLadingNumberCustomisationElement element = new BillOfLadingNumberCustomisationElement(parentCustomisation, strategy);
			Add(element);
			return element;
		}

		#endregion

		#region MaxGeneratedLength

		public int CalcMaxGeneratedLength()
		{
			int total = 0;

			foreach (BillOfLadingNumberCustomisationElement element in this)
			{
				if (element.Include)
				{
					total += element.Strategy.CalcMaxGeneratedLength(element);
				}
			}

			return total;
		}

		public event EventHandler MaxGeneratedLengthChanged;
		void OnMaxGeneratedLengthChanged()
		{
			if (MaxGeneratedLengthChanged != null)
			{
				MaxGeneratedLengthChanged(this, EventArgs.Empty);
			}
		}

		#endregion

		#region GetMaxUniversalOfficeCodeLength

		static int GetMaxUniversalOfficeCodeLength()
		{
			var filter = new ZDBOnlyQuery(ObjectFactory.GetType<IOrgCusCode>());
			filter.AddToFilter(OrgCusCodeSchema.OK_CodeType, "UOC");

			var orgProxiesQuery = new ZDBOnlyQuery(ObjectFactory.GetType<IOrgCusCode>());
			var companiesSubQuery = new ZDBOnlySubQuery(ObjectFactory.GetType<IGlbCompany>(), GlbCompanySchema.GC_OH_OrgProxy);
			var branchesSubQuery = new ZDBOnlySubQuery(ObjectFactory.GetType<IGlbBranch>(), GlbBranchSchema.GB_OH_OrgProxy);

			orgProxiesQuery.AddSubQuery(OrgCusCodeSchema.OK_OH, companiesSubQuery, JoinCondition.And);
			orgProxiesQuery.AddSubQuery(OrgCusCodeSchema.OK_OH, branchesSubQuery, JoinCondition.Or);

			filter.AddToFilter(orgProxiesQuery);

			var codes = new BusinessObjectFactory().Load<IOrgCusCode>(filter);
			if (!codes.Any())
			{
				return 0;
			}

			var maxLength = codes.Select(c => ((BusinessObject)c)[nameof(OrgCusCodeSchema.OK_CustomsRegNo)].ToString().Length).Max();
			return maxLength;
		}

		#endregion

		#region On Added / Removed

		protected override void OnAdded(BusinessObject bizOAdded)
		{
			BillOfLadingNumberCustomisationElement element = (BillOfLadingNumberCustomisationElement)bizOAdded;
			base.OnAdded(element);

			element.IncludeInfo.ValueChanged += maxGeneratedLengthChanged;
			element.DetailInfo.ValueChanged += maxGeneratedLengthChanged;
		}

		protected override void OnRemoved(BusinessObject bizO)
		{
			BillOfLadingNumberCustomisationElement element = (BillOfLadingNumberCustomisationElement)bizO;
			base.OnRemoved(element);

			element.IncludeInfo.ValueChanged -= maxGeneratedLengthChanged;
			element.DetailInfo.ValueChanged -= maxGeneratedLengthChanged;
		}

		void maxGeneratedLengthChanged(object sender, EventArgs e)
		{
			OnMaxGeneratedLengthChanged();
		}

		#endregion

		#region Implementation

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new BillOfLadingNumberCustomisationElement(parentCustomisation, new NullElementStrategy());
		}

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		protected override IComparer GetComparerForSort(PropertyDescriptor property, ListSortDirection direction)
		{
			if (property.Name == BillOfLadingNumberCustomisationElement.Schema.Order)
			{
				return new OrderComparer(direction != ListSortDirection.Ascending);
			}
			else
			{
				return base.GetComparerForSort(property, direction);
			}
		}

		#region OrderComparer

		class OrderComparer : IComparer, IComparer<BillOfLadingNumberCustomisationElement>
		{
			public OrderComparer(bool inverse)
			{
				this.inverse = inverse;
			}

			int CompareCore(byte x, byte y)
			{
				if (x == y)
				{
					return 0;
				}

				if (x == 0)
				{
					return 1;
				}

				if (y == 0)
				{
					return -1;
				}

				return x - y;
			}

			public int Compare(BillOfLadingNumberCustomisationElement x, BillOfLadingNumberCustomisationElement y)
			{
				int diff = CompareCore(x.Order, y.Order);
				return inverse ? -diff : diff;
			}

			#region IComparer Members

			int IComparer.Compare(object x, object y)
			{
				return Compare((BillOfLadingNumberCustomisationElement)x, (BillOfLadingNumberCustomisationElement)y);
			}

			#endregion

			readonly bool inverse;
		}

		#endregion

		#endregion

		#region IXmlSerializable Members

		public XmlSchema GetSchema()
		{
			throw new NotImplementedException();
		}

		public void ReadXml(XmlReader reader)
		{
			if (reader.IsEmptyElement)
			{
				reader.ReadStartElement(BillOfLadingNumberCustomisationElement.XmlConstants.Elements);
			}
			else
			{
				reader.ReadStartElement(BillOfLadingNumberCustomisationElement.XmlConstants.Elements);

				while (reader.Name == BillOfLadingNumberCustomisationElement.XmlConstants.Element)
				{
					string name = reader.GetAttribute(BillOfLadingNumberCustomisationElement.XmlConstants.key);
					BillOfLadingNumberCustomisationElement element = this[name] ?? throw new InvalidOperationException("Unknown element key: " + name);

					element.ReadXml(reader);
				}

				reader.ReadEndElement();
			}
		}

		public void WriteXml(XmlWriter writer)
		{
			foreach (BillOfLadingNumberCustomisationElement element in this)
			{
				if (element.Include)
				{
					element.WriteXml(writer);
				}
			}
		}

		#endregion

		readonly BillOfLadingNumberCustomisation parentCustomisation;
	}
}

#region Test
#if DEBUG

#region DisplayProxy

namespace Enterprise.Registry.Business
{
	[System.Diagnostics.DebuggerTypeProxy(typeof(_DisplayProxy))]
	public partial class BillOfLadingNumberCustomisationElementCollection
	{
		class _DisplayProxy
		{
			public _DisplayProxy(BillOfLadingNumberCustomisationElementCollection parent)
			{
				this.parent = parent;
			}

			[System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.RootHidden)]
			public BillOfLadingNumberCustomisationElement[] Elements
			{
				get { return parent.ToArray<BillOfLadingNumberCustomisationElement>(); }
			}

			readonly BillOfLadingNumberCustomisationElementCollection parent;
		}
	}
}

#endregion

#endif
#endregion
