using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.ASYCUDA.Business
{
	public class AsycudaArrivalHeader : ManifestBase.AsycudaArrivalHeader
		, Integration.Customs.ASYCUDA.IAsycudaArrivalHeader
		, ISelectionItem
		, IShortSequenceNumberLine
	{
		public AsycudaArrivalHeader(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public AsycudaManifestHeader ManifestHeader => manifestHeader ?? (manifestHeader = Factory.Load<AsycudaManifestHeader>(ATH_AMA_ManifestHeader));
		AsycudaManifestHeader manifestHeader;

		[RelatedBusinessObject("ManifestHeader")]
		public override ZGuid ATH_AMA_ManifestHeader
		{
			get => base.ATH_AMA_ManifestHeader;
			set
			{
				var oldValue = ATH_AMA_ManifestHeader;
				base.ATH_AMA_ManifestHeader = value;
				if (!IsCopying && oldValue != ATH_AMA_ManifestHeader)
				{
					if (!ATH_AMA_ManifestHeader.IsValid)
					{
						DetachedLine();
					}
					else
					{
						AttachedLine();
					}
					Header?.MarkAsNeedingValidation();
				}
			}
		}

		void DetachedLine()
		{
			var header = ManifestHeader;
			if (header != null)
			{
				SequenceGenerator.RecalculateWhenAboutToBeDetachedOrDeleted(this);
			}
		}

		void AttachedLine()
		{
			var header = ManifestHeader;
			if (header != null)
			{
				SequenceGenerator.RecalculateWhenAdded(this);
			}
		}

		AsycudaArrivalLineCollection arrivalDetails;

		[ChildEditable]
		public AsycudaArrivalLineCollection ArrivalDetails
		{
			get
			{
				if (arrivalDetails == null)
				{
					arrivalDetails = CreateNewArrivalLineCollection();
					RegisterEditableChildObject(arrivalDetails);
				}
				return arrivalDetails;
			}
		}

		public override void Delete()
		{
			DetachedLine();
			base.Delete();
			if (arrivalDetails != null)
			{
				arrivalDetails.DeleteAll();
			}
		}

		[ResourceStringData("ArrivalHeaders|B26E7EFE-D759-49A4-9082-1B6209D18B8E", Caption = "Flight No.")]
		public override ZString ATH_VoyageFlightNo
		{
			get => base.ATH_VoyageFlightNo;
			set => base.ATH_VoyageFlightNo = value;
		}

		[ResourceStringData("ArrivalHeaders|7CE8E7B9-08A9-48C9-B220-02270D057008", Caption = "Reference")]
		public override ZString ATH_Reference
		{
			get => base.ATH_Reference;
			set => base.ATH_Reference = value;
		}

		[ResourceStringData("ArrivalHeaders|86F1A480-E2F7-4190-8CB1-6B851E0BB852", Caption = "ETA")]
		public override ZDateTime ATH_ETAAtDischargePort
		{
			get => base.ATH_ETAAtDischargePort;
			set
			{
				base.ATH_ETAAtDischargePort = value;
				ArrivalDetails.ForEach(x => x.ATL_ExpectedQtyInfo.RefreshBinding());
			}
		}

		[ResourceStringData("ArrivalHeaders|86F1A480-E2F7-4190-8CB1-6B851E0BB852", Caption = "ETA")]
		public ZDateTime ETAAtDischargePortForShortFormat
		{
			get => ATH_ETAAtDischargePort;
			set => ATH_ETAAtDischargePort = value;
		}

		public ZPropertyInfo ETAAtDischargePortForShortFormatInfo => GetWrappedZPropertyInfo(nameof(ETAAtDischargePortForShortFormat), x => ATH_ETAAtDischargePortInfo);

		[ResourceStringData("ArrivalHeaders|0A4CEA18-5AF1-4553-913D-EE5BC520F8C3", Caption = "Reference Date")]
		public override ZDate ATH_ReferenceIssueDate
		{
			get => base.ATH_ReferenceIssueDate;
			set => base.ATH_ReferenceIssueDate = value;
		}

		#region ICanDelete Members

		public override bool CanDelete
		{
			get { return !HasMessageSent && base.CanDelete; }
		}

		public override MultilingualString ReasonForNotAbleToDelete
		{
			get
			{
				if (HasMessageSent)
				{
					return ResString.GetMultilingualString("076BA98E-C173-4EB0-B673-D554B09ABDA3", "Bills under this flight are already sent to Customs.");
				}

				return base.ReasonForNotAbleToDelete;
			}
		}

		bool HasMessageSent
		{
			get
			{
				return ManifestHeader.Messages.OfType<EDIMessage>().Any(x => x.EM_LinkTable == AsycudaArrivalHeader.Schema.TableName && x.EM_LinkUniqueID == PK);
			}
		}

		#endregion

		protected virtual AsycudaArrivalLineCollection CreateNewArrivalLineCollection() => new AsycudaArrivalLineCollection(this);

		public new IBusinessObjectCollection<AsycudaTransferHeader> TransferHeaders => (IBusinessObjectCollection<AsycudaTransferHeader>)base.TransferHeaders;
		protected override ManifestBase.IAsycudaTransferHeaderCollection<ManifestBase.AsycudaTransferHeader> CreateNewAsycudaTransferHeaderCollection() => new ManifestBase.AsycudaTransferHeaderCollection<AsycudaTransferHeader>(this);

		public new AsycudaArrivalHeaderValidation Validation => (AsycudaArrivalHeaderValidation)base.Validation;
		protected override ManifestBase.AsycudaArrivalHeaderValidation GetNewValidation() => new AsycudaArrivalHeaderValidation(this);

		public new AsycudaArrivalHeaderLookups Lookups => (AsycudaArrivalHeaderLookups)base.Lookups;
		protected override ManifestBase.AsycudaArrivalHeaderLookups GetNewLookups() => new AsycudaArrivalHeaderLookups(this);

		#region ISelectedItem

		ZGuid ISelectionItem.PK => PK;

		string ISelectionItem.SelectionDescription(bool showStatus)
		{
			return ATH_VoyageFlightNo;
		}

		#endregion

		#region IShortSequenceNumberLine Members

		ZGuid ISequenceNumberLine.FKToHeader => ATH_AMA_ManifestHeader;

		ZShort ISequenceNumberLine<ZShort>.SequenceNumber
		{
			get { return ATH_ArrivalSequence; }
			set { ATH_ArrivalSequence = value; }
		}

		#endregion

		[ReadOnlyMember(nameof(ATH_ArrivalSequence_ReadOnly))]
		[ResourceStringData("ArrivalHeaders|1E88F8B3-1AFE-4EAA-A89C-837890A3B27A", Caption = "Leg Order")]
		public override ZShort ATH_ArrivalSequence
		{
			get { return base.ATH_ArrivalSequence; }
			set
			{
				var oldValue = ATH_ArrivalSequence;
				base.ATH_ArrivalSequence = ShortSequenceNumberGenerator.EnsureValidSequenceNumber(value);
				if (!IsCopying)
				{
					SequenceGenerator.RecalculateWhenRenumbered(this, oldValue);
				}
			}
		}

		protected virtual bool ATH_ArrivalSequence_ReadOnly => true;

		public ShortSequenceNumberGenerator SequenceGenerator => sequenceGenerator ?? (sequenceGenerator = new ShortSequenceNumberGenerator(() => ManifestHeader.ArrivalHeaders));
		ShortSequenceNumberGenerator sequenceGenerator;
	}
}
