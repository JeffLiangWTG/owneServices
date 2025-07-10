using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Extensions;
using Enterprise.Customs.Business.FetchStrategies;
using Enterprise.Customs.ExitControlBase.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.EU.ExitControl.Business
{
	[CodeProperty(nameof(MessageCodeForEdocs))]
	[DescriptionProperty(nameof(MessageDescriptionForEdocs))]
	public class CusExitConsignment : ExitControlBase.Business.CusExitConsignment
		, Integration.Customs.EUExitControl.ICusExitConsignment
		, Integration.Customs.ICusSupportingInfoTypeSupporter
		, IDocManagerSupport
		, IUcc6ValueProvider
	{
		public CusExitConsignment(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new CusExitHeader Header => Factory.Load<CusExitHeader>(CXC_CXH_Header);

		public new CusExitConsignmentLookups Lookups => (CusExitConsignmentLookups)base.Lookups;

		public new CusExitConsignmentValidation Validation => (CusExitConsignmentValidation)base.Validation;

		protected override ExitControlBase.Business.CusExitConsignmentLookups GetNewLookups() => new CusExitConsignmentLookups(this);

		protected override ExitControlBase.Business.CusExitConsignmentValidation GetNewValidation() => new CusExitConsignmentValidation(this);

		public new ICusExitConsignmentItemCollection<CusExitConsignmentItem> CusExitConsignmentItems => (ICusExitConsignmentItemCollection<CusExitConsignmentItem>)base.CusExitConsignmentItems;

		protected override ICusExitConsignmentItemCollection<ExitControlBase.Business.CusExitConsignmentItem> CreateNewCusExitConsignmentItemCollection() => new CusExitConsignmentItemCollection<CusExitConsignmentItem>(this);

		protected override ZString HumanReadableNameCore => Res.GetString("{47FFDB81-AC18-46E3-9C39-0BAB5F34D586}", "Consignment");

		public static readonly CusExitConsignmentTypeDecider TypeDecider = new CusExitConsignmentTypeDecider();

		protected override int MaxItemCountCore => 9999;

		[ReadOnlyMember(nameof(IsMovementReferenceReadOnly))]
		[ResourceStringData("2A75401D-BB7A-4242-9EF7-30F56A6A4CDC", Caption = "MRN")]
		public override ZString CXC_MovementReference
		{
			get => base.CXC_MovementReference;
			set => base.CXC_MovementReference = value;
		}

		protected virtual bool IsMovementReferenceReadOnly => false;

		[ResourceStringData("206FC050-F55B-4FE8-9180-7D2394319D81", Caption = "LRN")]
		public override ZString CXC_LocalReference
		{
			get => base.CXC_LocalReference;
			set => base.CXC_LocalReference = value;
		}

		[ResourceStringData("DDE2B655-B3CE-4D62-B9FF-14A7B2C4AB31", Caption = "Status")]
		[List(nameof(Lookups) + "." + nameof(CusExitConsignmentLookups.StatusList))]
		[ReadOnly(true)]
		public override ZString CXC_Status
		{
			get => base.CXC_Status;
			set => base.CXC_Status = value;
		}

		[ResourceStringData("5C5FA5D4-FEFE-4DB3-BE5A-7A6170561390", Caption = "Status Description", MediumCaption = "Status Desc.", ShortCaption = "Desc.")]
		public ZString StatusDescription
		{
			get
			{
				var result = ZString.Empty;
				if (!CXC_Status.IsEmpty)
				{
					result = Lookups.StatusList.GetDescriptionFromCode(CXC_Status);
				}
				return result;
			}
		}

		[ResourceStringData("168C46C7-0723-469A-B1BA-9EED614E67C0", Caption = "Reference Number UCR", MediumCaption = "Ref. No. UCR")]
		public override ZString CXC_UniqueConsignmentReference
		{
			get => base.CXC_UniqueConsignmentReference;
			set => base.CXC_UniqueConsignmentReference = value;
		}

		public override bool CanDelete => !Header.CusExitReports.Any(r => r.CER_CXC_Consignment == PK);

		public override MultilingualString ReasonForNotAbleToDelete => ResString.GetMultilingualString("AA5B6028-1A25-44DB-9576-CF5D6544902C", "Entry cannot be deleted as it is referenced in an Exit Report.");

		IActiveBusinessObjectCollection<Integration.Customs.EUExitControl.ICusExitConsignmentItem> Integration.Customs.EUExitControl.ICusExitConsignment.CusExitConsignmentItems => CusExitConsignmentItems;

		public EDIMessageCollection Messages
		{
			get
			{
				if (messages == null)
				{
					messages = new EDIMessageCollection(this, Factory);
					messages.Load();
					messages.IsManagedForDataRefresh = true;
				}
				return messages;
			}
		}
		EDIMessageCollection messages;

		public ZString MessageDescriptionForEdocs => CXC_MovementReference.IsEmpty ? "LRN:" + CXC_LocalReference : "MRN:" + CXC_MovementReference;

		public ZString MessageCodeForEdocs => CXC_MovementReference.IsEmpty ? CXC_LocalReference : CXC_MovementReference;

		public DocManagerInfo DocManagerInfo => docManagerInfo ?? (docManagerInfo = new DocManagerInfo(this, Core.Constants.DocManagerCodes.CustomsExitConsignment));
		DocManagerInfo docManagerInfo;

		public virtual bool ConsignmentItemsRequiredToCreateCusExitReport => true;

		public bool IsUCC6 => IsUCC6Core;
		protected virtual bool IsUCC6Core => Header?.IsUCC6 ?? false;

		internal bool HasMoreThanOneConsignmentItemMatching(ZShort lineNumber) => MoreThanOneConsignmentItemMatchingLineNumberSet.Contains(lineNumber);

		HashSet<ZShort> MoreThanOneConsignmentItemMatchingLineNumberSet => Factory.GetValue(ref moreThanOneConsignmentItemMatchingLineNumberSet, () =>
		{
			var result = new HashSet<ZShort>();
			var processedSet = new HashSet<ZShort>();
			foreach (var lineNumber in CusExitConsignmentItems.Select(x => x.CCI_LineNumber).Where(x => x > ZShort.Zero))
			{
				if (!processedSet.Add(lineNumber))
				{
					result.Add(lineNumber);
				}
			}
			return result;
		});
		CachedProperty<HashSet<ZShort>> moreThanOneConsignmentItemMatchingLineNumberSet;

		#region ICusSupportingInfoTypeSupporter Implementation

		IDictionary<ZString, Type> Integration.Customs.ICusSupportingInfoTypeSupporter.GetCusSupportingInfoTypes() => GetCusSupportingInfoTypes();

		protected virtual IDictionary<ZString, Type> GetCusSupportingInfoTypes() => new Dictionary<ZString, Type>
		{
		};

		IEnumerable<IBusinessObjectFetchStrategy> IAdditionalBusinessObjectFetchStrategyProvider.GetFetchStrategies() => GetAdditionalBusinessObjectFetchStrategies();

		protected virtual IEnumerable<IBusinessObjectFetchStrategy> GetAdditionalBusinessObjectFetchStrategies()
		{
			yield return new CusSupportingInfoTypeSupporterFetchStrategy(this);
		}

		#endregion

	}
}
