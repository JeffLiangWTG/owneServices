using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.JP.AFR.Business;
using Enterprise.DocumentWrappers;
using Enterprise.MasterFiles.Business;
using CoreConstants = Enterprise.Core.Constants;

namespace Enterprise.Customs.JP.AFR.DocumentWrappers
{
	public class DocJPAFRContainer : DocBaseWrapper
	{
		const string SealSeperator = ";";

		#region ctor

		DocJPAFRContainer(JPAFRContainer container, DocJPAFRBills bill, int i, BusinessObjectFactory factoryToWrap)
			: base(container, factoryToWrap)
		{
			Argument.NotNull(container, "container");
			this.bill = Argument.NotNull(bill, "bill");
			this.containerSequence = i;
		}

		DocJPAFRContainer(DocJPAFRBills bill, BusinessObjectFactory factoryToWrap)
			: base(null, factoryToWrap)
		{
			this.bill = bill;
		}

		public static DocJPAFRContainer New(JPAFRContainer container, DocJPAFRBills bill, int i, BusinessObjectFactory factoryToWrap)
		{
			DocJPAFRContainer result = null;

			if (container != null && bill != null)
			{
				result = new DocJPAFRContainer(container, bill, i, factoryToWrap);
			}

			return result;
		}

		public static DocJPAFRContainer New(JPAFRContainer container, BusinessObjectFactory factoryToWrap)
		{
			DocJPAFRContainer result = null;

			if (container != null)
			{
				var bill = container.Bill;
				var docBill = bill != null ? DocJPAFRBills.New(bill, factoryToWrap) : null;
				var containerCollection = bill != null ? bill.Containers : null;
				var index = containerCollection != null ? containerCollection.IndexOf(container) : -1;
				if (docBill != null && index >= 0)
				{
					result = new DocJPAFRContainer(container, docBill, index + 1, factoryToWrap);
				}
			}

			return result;
		}

		public static DocJPAFRContainer NewDummy(DocJPAFRBills bill, BusinessObjectFactory businessObjectFactory)
		{
			return DummyDocJPAFRContainer.New(Argument.NotNull(bill, "Bill"), businessObjectFactory);
		}

		#endregion

		#region Related Business Objects

		JPAFRContainer WrappedContainer
		{
			get { return (JPAFRContainer)WrappedObject; }
		}

		RefContainer ContainerType
		{
			get { return containerType ?? (containerType = WrappedContainer.ContainerType); }
		}
		RefContainer containerType;

		public DocJPAFRBills Bill
		{
			get { return this.bill; }
		}
		readonly DocJPAFRBills bill;

		#endregion

		#region Container Info Section

		public virtual ZString ContainerNumber
		{
			get { return WrappedContainer.JPC_ContainerNum; }
		}

		public virtual ZString ContainerSeal
		{
			get
			{
				var seal1 = WrappedContainer.JPC_Seal1;
				var seal2 = WrappedContainer.JPC_Seal2;
				var seperator = (seal1.IsEmpty || seal2.IsEmpty) ? string.Empty : SealSeperator;
				return string.Format("{0}{1}{2}", seal1, seperator, seal2);
			}
		}

		public virtual ZString ContainerSizeCode
		{
			get { return ContainerType != null ? TranslateContainerLengthCode(ContainerType.RC_Length) + TranslateContainerHeightCode(ContainerType.RC_Height) : string.Empty; }
		}

		public virtual ZString ContainerTypeCode
		{
			get
			{
				var containerCategory = ContainerType == null ? ZString.Empty : ContainerType.RC_ContainerType;
				return TranslateContainerTypeCode(containerCategory);
			}
		}

		public virtual ZString ContainerOwnershipCode
		{
			get { return WrappedContainer.JPC_OwnershipCode; }
		}

		public virtual ZInt ContainerSequence
		{
			get { return containerSequence; }
		}
		readonly int containerSequence;

		#endregion

		#region Implementation

		static string TranslateContainerTypeCode(ZString containerCategory)
		{
			switch (containerCategory)
			{
				case "":
					return ZString.Empty;
				case CoreConstants.ContainerTypes.DryStorage:
					return "GP";
				case CoreConstants.ContainerTypes.Refrigerated:
					return "RT";
				case CoreConstants.ContainerTypes.OpenTop:
					return "UT";
				case CoreConstants.ContainerTypes.FlatRack:
					return "PF";
				case CoreConstants.ContainerTypes.Bolster:
					return "PL";
				case CoreConstants.ContainerTypes.Tank:
					return "TN";
				default:
					return "SN";
			}
		}

		static string TranslateContainerLengthCode(ZDecimal length)
		{
			var result = string.Empty;
			if (length >= 10m && length < 20m)
			{
				result = "1";
			}
			else if (length >= 20m && length < 30m)
			{
				result = "2";
			}
			else if (length >= 40m && length < 50m)
			{
				result = "4";
			}
			else
			{
				result = "9";
			}
			return result;
		}

		static string TranslateContainerHeightCode(ZDecimal height)
		{
			var result = string.Empty;
			if (height >= 4m && height <= 4.25m)
			{
				result = "8";
			}
			else if (height >= 8m && height < 8.5m)
			{
				result = "0";
			}
			else if (height >= 8.5m && height < 9m)
			{
				result = "2";
			}
			else if (height >= 9m && height < 9.5m)
			{
				result = "4";
			}
			else if (height == 9.5m)
			{
				result = "5";
			}
			else if (height >= 9.5m)
			{
				result = "6";
			}
			else
			{
				result = "9";
			}
			return result;
		}

		#endregion

		#region DummyDocJPAFRContainer

		public class DummyDocJPAFRContainer : DocJPAFRContainer
		{
			DummyDocJPAFRContainer(DocJPAFRBills bill, BusinessObjectFactory factoryToWrap)
				: base(bill, factoryToWrap)
			{
			}

			public static DocJPAFRContainer New(DocJPAFRBills bill, BusinessObjectFactory factoryToWrap)
			{
				DocJPAFRContainer result = null;

				if (bill != null)
				{
					result = new DummyDocJPAFRContainer(bill, factoryToWrap);
				}

				return result;
			}

			public override ZString ContainerOwnershipCode
			{ get { return ZString.Empty; } }

			public override ZString ContainerTypeCode
			{ get { return ZString.Empty; } }

			public override ZString ContainerSizeCode
			{ get { return ZString.Empty; } }

			public override ZString ContainerSeal
			{ get { return ZString.Empty; } }

			public override ZString ContainerNumber
			{ get { return ZString.Empty; } }

			public override ZInt ContainerSequence
			{ get { return 1; } }
		}

		#endregion
	}
}
