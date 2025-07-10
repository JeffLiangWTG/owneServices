using System;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.NetworkVisualisation.Business;
using CargoWise.NetworkVisualisation.Integration;
using CargoWise.PAVE.Common.Interfaces;
using CargoWise.Types;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Integration;

namespace Enterprise.BufferManagement.NetworkVisualisation.Business
{
	[DebuggerDisplay("{DisplayText}")]
	public class BMNCNAttachment : AutoBMNCNAttachment,
		IEntityRelationship,
		IEntityRelationshipLayout,
		IBMNCNAttachment,
		ILink,
		IApprovable,
		INotifyPropertyChanged
	{
		public BMNCNAttachment(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region BusinessObject Overrides

		public override bool IsSavedByFactory
		{
			get
			{
				if (base.IsSavedByFactory)
				{
					if (!IsDeleted)
					{
						var defaultDiagram = OwnerShape as BMNCNShapeDefaultDiagram;
						if (defaultDiagram != null)
						{
							return new[] { defaultDiagram, ToShape, FromShape }.WhereNotNull().All(shape => shape.IsInDatabase || shape.IsSavedByFactory);
						}
					}

					return true;
				}
				else
				{
					return false;
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1121:DoNotIncludeColumnValuesOrNamesInErrorReporterKey", Justification = "Baseline")]
		public override void OnSaving()
		{
			base.OnSaving();

			if (IsInvalidDependencyAttachment()
				&& stackTraceForErrorReporting != null)
			{
				ErrorReporter.ReportOnce("A BMNCNAttachment was created with an empty BNA_BNS_FromShape:" + System.Environment.NewLine + stackTraceForErrorReporting.ToString());
			}
		}

		public override void Delete()
		{
			if (!IsDeleted && IsApproved)
			{
				throw new CannotDeleteException("This arrow has been approved and cannot be deleted. It should be decoupled instead.");
			}

			var buffer = !IsDeleted ? this.GetBuffer() : null;

			base.Delete();

			if (buffer != null)
			{
				buffer.Delete();
			}
		}

		protected override ZString HumanReadableNameCore
		{
			get
			{
				if (IsResourceDependency)
				{
					return Res.GetString("2cce7f87-6492-4573-bfa8-a3bcef50a633", "Resource Dependency");
				}
				else if (IsDependencyLink)
				{
					return Res.GetString("f0e6e1cc-816a-4030-9374-fc3fab80e247", "Dependency Arrow");
				}
				else
				{
					return base.HumanReadableNameCore.ToString();
				}
			}
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();

			stackTraceForErrorReporting = new StackTrace(false);
		}

		StackTrace stackTraceForErrorReporting;

		#endregion

		#region Properties

		#region BNA_BNS_FromShape

		[RelatedBusinessObject("FromShape")]
		public override ZGuid BNA_BNS_FromShape
		{
			get { return base.BNA_BNS_FromShape; }
			set { base.BNA_BNS_FromShape = value; }
		}

		#endregion

		#region BNA_BNS_ToShape

		[RelatedBusinessObject("ToShape")]
		public override ZGuid BNA_BNS_ToShape
		{
			get { return base.BNA_BNS_ToShape; }
			set { base.BNA_BNS_ToShape = value; }
		}

		#endregion

		#region BNA_BNS_Owner

		[RelatedBusinessObject("OwnerShape")]
		public override ZGuid BNA_BNS_Owner
		{
			get { return base.BNA_BNS_Owner; }
			set { base.BNA_BNS_Owner = value; }
		}

		#endregion

		#region BNA_FP_ProcessHeaderLink

		[RelatedBusinessObject("ProcessHeaderLink")]
		public override ZGuid BNA_FP_ProcessHeaderLink
		{
			get { return base.BNA_FP_ProcessHeaderLink; }
			set
			{
				base.BNA_FP_ProcessHeaderLink = value;
				RefreshAppearanceProperties();
			}
		}

		#endregion

		#region BNA_GS_NKApprovedBy

		public override ZString BNA_GS_NKApprovedBy
		{
			get { return base.BNA_GS_NKApprovedBy; }
			set
			{
				base.BNA_GS_NKApprovedBy = value;
				RefreshAppearanceProperties();
			}
		}

		#endregion

		#region BNA_IsDecouple

		public override ZBool BNA_IsDecouple
		{
			get { return base.BNA_IsDecouple; }
			set
			{
				base.BNA_IsDecouple = value;
				RefreshAppearanceProperties();
			}
		}

		#endregion

		#region BNA_Type

		[List("Lookups.Types")]
		public override ZString BNA_Type
		{
			get { return base.BNA_Type; }
			set
			{
				base.BNA_Type = value;
				RefreshAppearanceProperties();
			}
		}

		#endregion

		#endregion

		#region New Properties

		public bool IsRealEntityAttachmentWithNoProcessHeaderLink
		{
			get { return FromShape != null && ToShape != null && FromShape.IsLinkedToRealEntity && ToShape.IsLinkedToRealEntity && ProcessHeaderLink == null; }
		}

		public bool IsResourceDependency
		{
			get { return BNA_Type == AttachmentTypeList.Codes.ResourceDependency; }
		}

		public bool IsDependencyLink
		{
			get { return !BNA_BNS_FromShape.IsEmpty && !BNA_BNS_ToShape.IsEmpty && !BNA_FP_ProcessHeaderLink.IsEmpty; }
		}

		public bool IsArrow
		{
			get { return BNA_Type == AttachmentTypeList.Codes.ResourceDependency || BNA_Type == AttachmentTypeList.Codes.Dependency; }
		}

		#endregion

		#region Related Business Objects

		public BMNCNShape FromShape
		{
			get { return Factory.Load<BMNCNShape>(BNA_BNS_FromShape); }
		}

		public BMNCNShape ToShape
		{
			get { return Factory.Load<BMNCNShape>(BNA_BNS_ToShape); }
		}

		public BMNCNShape OwnerShape
		{
			get { return Factory.Load<BMNCNShape>(BNA_BNS_Owner); }
		}

		public ProcessHeaderLink ProcessHeaderLink
		{
			get { return Factory.Load<ProcessHeaderLink>(BNA_FP_ProcessHeaderLink); }
		}

		#endregion

		#region IEntityRelationship Members

		IProposedNetworkEntity IEntityRelationship.From
		{
			get { return FromShape; }
			set
			{
				var shape = value as BMNCNShape;
				if (shape != null)
				{
					BNA_BNS_FromShape = shape.PK;
				}
				else
				{
					BNA_BNS_FromShape = ZGuid.Empty;
				}
			}
		}

		IProposedNetworkEntity IEntityRelationship.To
		{
			get { return ToShape; }
			set
			{
				var shape = value as BMNCNShape;
				if (shape != null)
				{
					BNA_BNS_ToShape = shape.PK;
				}
				else
				{
					BNA_BNS_ToShape = ZGuid.Empty;
				}
			}
		}

		string IEntityRelationship.DisplayText
		{
			get { throw new InvalidOperationException(); }
		}

		public string DisplayText
		{
			get
			{
				var result = this.GetDisplayName();

				if (IsResourceDependency)
				{
					result = Res.GetString("9350eafa-3fbf-4e13-8597-495bed142511", "Resource Dependency:") + System.Environment.NewLine + result;
				}

				return result;
			}
		}

		#endregion

		#region IEntityRelationshipLayout Members

		public string BackColor
		{
			get
			{
				return IsResourceDependency ? BMConstants.ResourceDependencyArrowColorName
					: BNA_IsDecouple ? BMConstants.DecoupledArrowColorName
						: IsRealEntityAttachmentWithNoProcessHeaderLink ? BMConstants.DependencyArrowWithoutLinkColorName
							: BMConstants.NecessityDependencyArrowColorName;
			}
		}

		public bool IsVisible
		{
			get { return !BNA_IsHidden; }
		}

		public ArrowAppearance Appearance
		{
			get { throw new InvalidOperationException(); }
		}

		#endregion

		#region Clone

		protected override bool SupportsCloneCore()
		{
			return !IsInvalidDependencyAttachment();
		}

		bool IsInvalidDependencyAttachment()
		{
			return BNA_Type == AttachmentTypeList.Codes.Dependency && BNA_BNS_FromShape == ZGuid.Empty;
		}

		#endregion

		#region ILink Members

		Guid ILink.PK
		{
			get { return PK.ToGuid(); }
		}

		ILinkEntity ILink.Owner
		{
			get { return OwnerShape; }
		}

		ILinkEntity ILink.From
		{
			get { return FromShape; }
		}

		ILinkEntity ILink.To
		{
			get { return ToShape; }
		}

		bool ILink.IsBufferedAttachment => IsBuffered;

		#endregion

		#region IApprovable Members

		public bool IsApproved
		{
			get { return !BNA_GS_NKApprovedBy.IsEmpty; }
		}

		public void Approve(string approverCode)
		{
			ApproveCore(approverCode);
		}

		public void UnApprove()
		{
			ApproveCore(ZString.Empty);
		}

		void ApproveCore(string approverCode)
		{
			BNA_GS_NKApprovedBy = approverCode;
		}

		#endregion

		#region INotifyPropertyChanged Members

		void RefreshAppearanceProperties()
		{
			OnPropertyChanged(nameof(Appearance)); // Property name
			OnPropertyChanged(nameof(BackColor)); // Property name
			OnPropertyChanged(nameof(DisplayText)); // Property name
		}

		protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
		{
			if (PropertyChanged != null)
			{
				PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;

		#endregion

		#region IBMNCNAttachment Members

		public void Decouple()
		{
			BNA_IsDecouple = ZBool.True;

			var dependency = ProcessHeaderLink;
			if (dependency != null)
			{
				dependency.Delete();
			}

			BNA_FP_ProcessHeaderLink = ZGuid.Empty;
		}

		public bool IsBuffered
		{
			get
			{
				return this.GetBuffer() != null
					&& FromShape != null && !FromShape.IsBufferShape
					&& ToShape != null && !ToShape.IsBufferShape;
			}
		}

		#endregion

		#region Scheduling

		public static bool IsApplicableForScheduling(BMNCNAttachment attachment)
		{
			switch (attachment.BNA_Type)
			{
				case AttachmentTypeList.Codes.Dependency:
				case AttachmentTypeList.Codes.ResourceDependency:
					return true;

				default:
					return false;
			}
		}

		#endregion
	}
}
