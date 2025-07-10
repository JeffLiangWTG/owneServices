using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.JP.AFR.Business;
using Enterprise.DocumentWrappers;
using CoreConstants = Enterprise.Core.Constants;

namespace Enterprise.Customs.JP.AFR.DocumentWrappers
{
	public class DocJPAFRBills : DocBaseWrapper
	{
		DocJPAFRBills(JPAFRBills bill, BusinessObjectFactory factoryToWrap)
			: base(bill, factoryToWrap)
		{
			Argument.NotNull(bill, "bill");
		}

		public static DocJPAFRBills New(JPAFRBills bill, BusinessObjectFactory factoryToWrap)
		{
			return bill != null ? new DocJPAFRBills(bill, factoryToWrap) : null;
		}

		#region Reference Business Objects

		JPAFRBills WrappedBill
		{
			get { return (JPAFRBills)WrappedObject; }
		}

		#endregion

		#region Bill Infomation

		public ZString PlaceOfDelivery
		{
			get { return WrappedBill.JPB_RL_NKDelivery; }
		}

		public ZString PlaceOfShipment
		{
			get { return WrappedBill.JPB_RL_NKFinalDestination; }
		}

		public ZString Consignor
		{
			get
			{
				var consignor = WrappedBill.Consignor;
				return consignor == null ? string.Empty : consignor.AddressAsASingleLine + System.Environment.NewLine + consignor.E2_Phone;
			}
		}

		public ZString Consignee
		{
			get
			{
				var consignee = WrappedBill.Consignee;
				return consignee == null ? string.Empty : consignee.AddressAsASingleLine + System.Environment.NewLine + consignee.E2_Phone;
			}
		}

		public ZString NotifyParty
		{
			get
			{
				var notifyParty = WrappedBill.NotifyParty1;
				return notifyParty == null ? string.Empty : notifyParty.AddressAsASingleLine + System.Environment.NewLine + notifyParty.E2_Phone;
			}
		}

		public ZString GoodsDescription
		{
			get { return WrappedBill.JPB_GoodsDescription; }
		}

		public ZString MarksAndNumber
		{
			get { return WrappedBill.JPB_MarksAndNumbers; }
		}

		public ZString HSCode
		{
			get { return WrappedBill.JPB_Tariff; }
		}

		public ZString HouseBillNumber
		{
			get { return WrappedBill.JPB_BillNumber; }
		}

		public ZString PackageInfo
		{
			get { return string.Format("{0} {1}", WrappedBill.JPB_ManifestQty, TranslatePackageUnit(WrappedBill.JPB_ManifestUQ)); }
		}

		public ZString VolumeInfo
		{
			get { return string.Format("{0} {1}", WrappedBill.JPB_Volume, TranslateVolumeUnit(WrappedBill.JPB_VolumeUQ)); }
		}

		public ZString GrossWeightInfo
		{
			get { return string.Format("{0} {1}", WrappedBill.JPB_GrossWeight, TranslateWeightUnit(WrappedBill.JPB_GrossWeightUQ)); }
		}

		public ZString DangerousGoodIMDG
		{
			get
			{
				var dangerousGood = WrappedBill.Substance;
				return dangerousGood != null ? dangerousGood.DG_Class : ZString.Empty;
			}
		}

		public ZString DangerousGoodUNDG
		{
			get { return WrappedBill.JPB_DG_NKSubstance; }
		}

		#endregion

		#region Container Collection

		public DocJPAFRContainerCollection Containers
		{
			get
			{
				var result = new DocJPAFRContainerCollection(Factory);
				for (int i = 0; i < WrappedBill.Containers.Count; i++)
				{
					result.Add(DocJPAFRContainer.New(WrappedBill.Containers[i], this, i + 1, Factory));
				}
				return result;
			}
		}

		#endregion

		#region Implementation

		ZString TranslatePackageUnit(string code)
		{
			var result = Factory.GetCachedValue<PackageTypeList>().GetDescriptionFromCode(code);
			result = string.IsNullOrEmpty(result) ? code : result;
			return result;
		}

		static ZString TranslateVolumeUnit(ZString code)
		{
			switch (code)
			{
				case CoreConstants.Volume.CubicMetres:
					return "MTQ";
				case CoreConstants.Volume.CubicFeet:
					return "FTQ";
				case "BF":
					return "BFT";
				default:
					return code;
			}
		}

		static ZString TranslateWeightUnit(ZString code)
		{
			switch (code)
			{
				case CoreConstants.Weight.Kilograms:
					return "KGM";
				case CoreConstants.Weight.Pounds:
					return "LBR";
				case CoreConstants.Weight.Tonnes:
					return "TNE";
				default:
					return code;
			}
		}

		#endregion

	}
}
