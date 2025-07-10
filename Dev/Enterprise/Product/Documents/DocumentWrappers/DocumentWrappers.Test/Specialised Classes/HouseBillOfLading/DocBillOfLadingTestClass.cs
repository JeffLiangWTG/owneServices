using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentWrappers.Resources.Handlers;

namespace Enterprise.DocumentWrappers.Freight.Testing
{
	public class DocBillOfLadingTestClass : DocBillOfLading
	{
		public DocBillOfLadingTestClass(DocForwardingShipment shipment, ZGuid shipmentPK)
			: base(shipment)
		{
		}

		public ZString GetContainerTypeCountTestMethod()
		{
			return base.GetContainerTypeCount();
		}

		public DocPackLinesCollection MasterCoLoadPackLinesTestMethod
		{
			get
			{
				return base.MasterCoLoadPackLines;
			}
		}

		public ZString GetPackageCountTestMethod()
		{
			return base.GetPackageCount();
		}
		public new ZString ExtractBlock(ref ZString source, int blockSize)
		{
			return base.ExtractBlock(ref source, blockSize);
		}
		public ZString GetSpaces(ZInt numberOfTimes)
		{
			return base.FillWithSpaces(numberOfTimes);
		}

		public DocTransportCollection GetTransportLegsForParticularType(ZString type)
		{
			return base.GetTransportPlanningForType(type);
		}
		public bool ShouldAlignChargesTestMethod()
		{
			return base.ShouldAlignCharges();
		}
		public ZString AddTotalLineToAlignedChargesTestMethod(ZString alignedChargesColumn, ZDecimal totalAmount, ZString currencyCodeUsed, ZInt maxLength)
		{
			return base.AddTotalLineToAlignedCharges(alignedChargesColumn, totalAmount, currencyCodeUsed, maxLength);
		}
		public string TotalLineConstant
		{
			get
			{
				return base.TOTALLINE;
			}
		}
		public string TotalHeadingConstant
		{
			get
			{
				return base.TOTALHEADING;
			}
		}
		public new ZString AlignToWidth(ZString value, ZInt maxWidth)
		{
			return base.AlignToWidth(value, maxWidth);
		}
		public new StringCollectionX ConvertToStringCollection(ZString value, ZInt maxWidth)
		{
			return base.ConvertToStringCollection(value, maxWidth);
		}

		public new List<DocBillofLadingContainer> FCLContainersHavingRefContainerType
		{
			get { return base.FCLContainersHavingRefContainerType.ToList(); }
		}

		public new DocBillofLadingContainerCollection Containers
		{
			get { return base.Containers; }
		}
		public new ZString FillWithSpaces(ZInt numberOfTimes)
		{
			return base.FillWithSpaces(numberOfTimes);
		}
		public new StringCollectionX ShipmentVolumeCollection
		{
			get
			{
				return base.ShipmentVolumeCollection;
			}
		}
		public new StringCollectionX ShipmentWeightCollection
		{
			get
			{
				return base.ShipmentWeightCollection;
			}
		}
		public new StringCollectionX GoodsDescriptionStringCollection
		{
			get
			{
				return base.GoodsDescriptionStringCollection;
			}
		}
		public new StringCollectionX MarksAndNumbersStringCollection
		{
			get
			{
				return base.MarksAndNumbersStringCollection;
			}
		}
		public new TextSection MainBodyBottomColumnTextSection
		{
			get { return base.MainBodyBottomColumnTextSection; }
		}
		public new TextSection FollowOnBodyBottomSection
		{
			get { return base.FollowOnBodyBottomSection; }
		}
		public new ZString GetMainBodyTopSection()
		{
			return base.GetMainBodyTopSection();
		}
		public new ZString GetMainBodyBottomSection()
		{
			return base.GetMainBodyBottomSection();
		}
		public new TextSection GetFollowOnBodyTopSection()
		{
			return base.GetFollowOnBodyTopSection();
		}
		public new TextSection GetFollowOnBodyBottomSection()
		{
			return base.GetFollowOnBodyBottomSection();
		}
		public new ZInt HeightNeededForTopSection
		{
			get
			{
				return base.HeightNeededForTopSection;
			}
		}
		public new ZInt MainBodyTopSectionHeight
		{
			get
			{
				return base.MainBodyTopSectionHeight;
			}
		}
		public new ZInt MainBodyBottomSectionHeight
		{
			get
			{
				return base.MainBodyBottomSectionHeight;
			}
		}
		public new ZInt FollowOnBodyTopSectionHeight
		{
			get
			{
				return base.FollowOnBodyTopSectionHeight;
			}
		}
		public new ZInt FollowOnBodyBottomSectionHeight
		{
			get
			{
				return base.FollowOnBodyBottomSectionHeight;
			}
		}

		public ImageHandler ImageHandlerForTest
		{
			get { return base.GetImageHandler(); }
		}

		public ZString BillTermsImagePathForTest
		{
			get { return BillTermsImagePath; }
		}
	}
}
