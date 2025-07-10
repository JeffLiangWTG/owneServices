using System.Drawing;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.Registry.Business;
using Res = DocumentWrappers.Res;

namespace Enterprise.DocumentWrappers
{
	public class DocCostsComparer : DocBaseWrapper
	{
		DocCostsComparer(CostsComparer comparer, BusinessObjectFactory factoryToWrap)
			: base(comparer, factoryToWrap)
		{
		}

		public static DocCostsComparer New(CostsComparer comparer, BusinessObjectFactory factoryToWrap)
		{
			return (comparer != null) ? new DocCostsComparer(comparer, factoryToWrap) : null;
		}

		CostsComparer Comparer
		{
			get { return (CostsComparer)WrappedObject; }
		}

		#region Properties

		public Image Logo
		{
			get { return DocumentBrandingImage ?? SystemDataRegistry.Instance.CompanyLogo.Value; }
		}

		public DocCostsComparerRateLineCollection RateLines
		{
			get { return new DocCostsComparerRateLineCollection(Comparer); }
		}

		public ZString ValidFromDate
		{
			get { return Comparer.ValidFromDate.ToShortDateString(); }
		}

		public ZString ValidToDate
		{
			get { return Comparer.ValidToDate.ToShortDateString(); }
		}

		public ZString Mode
		{
			get { return Comparer.Mode; }
		}

		public ZString ModeDescription
		{
			get { return Comparer.TransportModes.GetDescriptionFromCode(Mode); }
		}

		public ZString Container
		{
			get
			{
				RefContainer container = Factory.Load<RefContainer>(Comparer.ContainerType);
				return container != null ? container.RC_Code : ZString.Empty;
			}
		}

		public ZString Origin
		{
			get { return Comparer.Origin; }
		}

		public ZString Destination
		{
			get { return Comparer.Destination; }
		}

		public ZString Commodity
		{
			get { return Comparer.CommodityCode; }
		}

		public ZString ChargesBeingShown
		{
			get
			{
				if (Comparer.ShowAllCharges)
				{
					return Res.GetString("7bda8889-4803-44b6-8105-e78605988106", "All Freight Charges {0} Origin/Destination rates included", Comparer.ShowOriginDestination ? Res.GetString("b0cd4792-c4cc-471e-bce4-7add81ae265f", "with") : Res.GetString("e6d7c3f8-bdf0-4689-a4e3-cc1dabcb59b8", "without"));
				}

				if (Comparer.ShowOriginChargesOnly)
				{
					return Res.GetString("d761f57b-4bae-4789-a3ff-eaf09139951e", "Origin Charges Only");
				}

				if (Comparer.ShowDestinationChargesOnly)
				{
					return Res.GetString("28c381cc-401a-426e-86e1-62f4ef59a4e2", "Destination Charges Only");
				}

				if (Comparer.SingleChargeCodeComparisonOnly)
				{
					return Res.GetString("03ec1c1e-0a2f-4506-a542-590aacbc932b", "Only Comparison for '{0}' Charge Code", Factory.Load<AccChargeCode>(Comparer.ChargeCodePK).AC_Code);
				}

				return string.Empty;
			}
		}

		public ZString Currency
		{
			get { return Comparer.Currency; }
		}

		public ZString StarWarnings
		{
			get
			{
				bool haveCNwarning = false;
				bool haveExRateWarning = false;
				bool haveUDwarnings = false;

				foreach (DocCostsComparerRateLine line in RateLines)
				{
					if (line.HasUDwarnings)
					{
						haveUDwarnings = true;
					}

					if (line.HasCNwarning)
					{
						haveCNwarning = true;
					}

					if (line.HasExRateWarning)
					{
						haveExRateWarning = true;
					}
				}

				ZStringBuilder result = new ZStringBuilder();

				result.AppendIfNotEmpty(haveCNwarning ? "* " + CostsComparerEntry.GetComplexNatureRowWarning() : string.Empty);
				result.AppendIfNotEmpty(haveExRateWarning ? Res.GetString("edad65ef-60df-458a-843f-13b69419b4d9", "** No current Buy exchange rate to local currency") : string.Empty);
				result.AppendIfNotEmpty(haveUDwarnings ? "*** " + CostsComparerEntry.GetUnitIsDifferenToFreightChargeRowWarning() : string.Empty);

				return result.ToStringWithNewLineBetweenAppends();
			}
		}

		#endregion

		#region SummaryColumnCaptions

		ZString GetSummaryColumnCaption(int columnNumber)
		{
			return Comparer.SummaryItems.Count >= columnNumber
					? Comparer.SummaryItems.Keys[columnNumber - 1].ColumnCaption
					: ZString.Empty;
		}

		public ZString SummaryColumn1Caption
		{
			get { return GetSummaryColumnCaption(1); }
		}

		public ZString SummaryColumn2Caption
		{
			get { return GetSummaryColumnCaption(2); }
		}

		public ZString SummaryColumn3Caption
		{
			get { return GetSummaryColumnCaption(3); }
		}

		public ZString SummaryColumn4Caption
		{
			get { return GetSummaryColumnCaption(4); }
		}

		public ZString SummaryColumn5Caption
		{
			get { return GetSummaryColumnCaption(5); }
		}

		public ZString SummaryColumn6Caption
		{
			get { return GetSummaryColumnCaption(6); }
		}

		public ZString SummaryColumn7Caption
		{
			get { return GetSummaryColumnCaption(7); }
		}

		public ZString SummaryColumn8Caption
		{
			get { return GetSummaryColumnCaption(8); }
		}

		public ZString SummaryColumn9Caption
		{
			get { return GetSummaryColumnCaption(9); }
		}

		public ZString SummaryColumn10Caption
		{
			get { return GetSummaryColumnCaption(10); }
		}

		#endregion
	}
}
