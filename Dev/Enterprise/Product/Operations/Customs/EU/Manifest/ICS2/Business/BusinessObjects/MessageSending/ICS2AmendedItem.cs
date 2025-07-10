using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business
{
	public interface IAmendedItem
	{
		RequestHeader RequestHeader { get; }

		ZBool IsSelected { get; }

		ZString Identifier { get; }

		ZString ResponsibleMemberState { get; }
	}

	public sealed class ICS2AmendedItem : NonPersistentBusinessObject, IAmendedItem
	{
		public ICS2AmendedItem(ICS2AmendedItemsHeader header, RequestHeader requestHeader)
			: base(header.Factory)
		{
			Header = Argument.NotNull(header, nameof(header));
			RequestHeader = Argument.NotNull(requestHeader, nameof(requestHeader));
		}

		public ICS2AmendedItemsHeader Header { get; }

		public RequestHeader RequestHeader { get; }

		#region Properties

		[ResourceStringData("Enterprise.Customs.EU.Manifest.ICS2.Business.ICS2AmendedItem|IsSelected", Caption = "Is Selected")]
		public ZBool IsSelected
		{
			get => isSelected;
			set
			{
				if (IsSelected != value)
				{
					SetNonPersistentPropertyValue(IsSelectedInfo, ref isSelected, value);
					if (!IsValidationSuspended)
					{
						Validation.ValidateIsSelected();
					}
				}

				IsSelectedInfo.RefreshBinding();
			}
		}
		ZBool isSelected;

		public ZPropertyInfo IsSelectedInfo => GetZPropertyInfo(nameof(IsSelected));

		[ResourceStringData("Enterprise.Customs.EU.Manifest.ICS2.Business.ICS2AmendedItem|Identifier", Caption = "Identifier")]
		public ZString Identifier => RequestHeader.EUS_Identifier;

		public ZPropertyInfo IdentifierInfo => GetZPropertyInfo(nameof(Identifier));

		[ResourceStringData("Enterprise.Customs.EU.Manifest.ICS2.Business.ICS2AmendedItem|RequestTypeDescription", Caption = "Type")]
		public ZString RequestTypeDescription => RequestHeader.RequestTypeDescription;

		public ZPropertyInfo RequestTypeDescriptionInfo => GetZPropertyInfo(nameof(RequestTypeDescription));

		[ResourceStringData("Enterprise.Customs.EU.Manifest.ICS2.Business.ICS2AmendedItem|ResponsibleMemberState", Caption = "State")]
		public ZString ResponsibleMemberState => RequestHeader.EUS_MemberState;

		public ZPropertyInfo ResponsibleMemberStateInfo => GetZPropertyInfo(nameof(ResponsibleMemberState));

		[ResourceStringData("Enterprise.Customs.EU.Manifest.ICS2.Business.ICS2AmendedItem|RequestStatus", Caption = "Status")]
		public ZString RequestStatus => RequestHeader.EUS_Status;

		public ZPropertyInfo RequestStatusInfo => GetZPropertyInfo(nameof(RequestStatus));

		#endregion

		public ICS2AmendedItemValidation Validation => new ICS2AmendedItemValidation(this);

		protected override void RunPreSaveValidationCore() => Validation.ValidateAll();
	}
}
