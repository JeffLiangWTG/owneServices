using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.NetworkVisualisation.Integration;
using CargoWise.PAVE.Common.Interfaces;
using CargoWise.Types;

namespace Enterprise.BufferManagement.NetworkVisualisation.Business
{
	[DebuggerDisplay("{DisplayText}")]
	public class NetworkAttachment : NonPersistentBusinessObject,
		ILink,
		IEntityRelationshipLayout
	{
		public NetworkAttachment(BMNCNAttachment attachment, NetworkEntityCollection collection)
		{
			this.attachment = attachment;
			this.collection = collection;
			NotificationsChanged += NetworkAttachment_NotificationsChanged;
			Owner?.RegisterEditableChildObject(this);
		}

		void NetworkAttachment_NotificationsChanged(object sender, NotificationsChangedEventArgs e)
		{
			if (!attachment.IsDeleted)
			{
				foreach (var shape in RelatedShapes)
				{
					shape.OnPropertyChanged("EntityState");
					shape.OnPropertyChanged("HasNotifications");
				}
			}
		}

		readonly BMNCNAttachment attachment;
		readonly NetworkEntityCollection collection;

		ILink Link => attachment;
		IEntityRelationshipLayout Layout => attachment;

		#region Properties

		#region Entity Accessors

		public BMNCNAttachment Attachment => attachment;
		public ShapeNetworkEntity Owner => ownerEntity ?? (ownerEntity = Link.Owner != null ? collection.GetInstance(Link.Owner) : null);
		public ShapeNetworkEntity From => fromEntity ?? (fromEntity = Link.From != null ? collection.GetInstance(Link.From) : null);
		public ShapeNetworkEntity To => toEntity ?? (toEntity = Link.To != null ? collection.GetInstance(Link.To) : null);

		ShapeNetworkEntity ownerEntity, fromEntity, toEntity;

		public IEnumerable<ShapeNetworkEntity> RelatedShapes => new[] { Owner, From, To }.Where(s => s != null && !s.IsDeleted);

		#endregion

		#region Meaningful Boolean Flags

		public bool IsResourceDependency => attachment.IsResourceDependency;

		public bool IsNonApproved => Owner.Root.Shape.IsApproved && !attachment.IsApproved;

		#endregion

		#region Properties for Validation

		public bool IsDecouple
		{
			get { return attachment.BNA_IsDecouple; }
			set
			{
				attachment.BNA_IsDecouple = value;

				if (!IsValidationSuspended)
				{
					Validation.ValidateBackInTimeArrows();
				}
			}
		}

		public bool IsBackInTime
		{
			get
			{
				var from = From;
				var to = To;

				if (from != null && to != null && !from.IsNonScheduled && !to.IsNonScheduled)
				{
					var root = from.Root;

					if (root.IsScaled)
					{
						return from.X + from.Width > to.X;
					}
				}

				return false;
			}
		}

		#endregion

		#endregion

		#region ILink

		Guid ILink.PK => PK.ToGuid();

		ILinkEntity ILink.Owner => Owner;

		ILinkEntity ILink.From => From;

		ILinkEntity ILink.To => To;

		bool ILink.IsBufferedAttachment => false;

		#endregion

		#region IEntityRelationshipLayout

		public string BackColor
		{
			get { return Layout.BackColor; }
		}

		public bool IsVisible
		{
			get { return Layout.IsVisible; }
		}

		public ArrowAppearance Appearance
		{
			get { return IsNonApproved ? ArrowAppearance.Dashed : attachment.BNA_IsDecouple ? ArrowAppearance.Dotted : ArrowAppearance.Normal; }
		}

		IProposedNetworkEntity IEntityRelationship.From
		{
			get { return Layout.From != null ? collection.GetInstance(Layout.From) : null; }
			set { Layout.From = (BMNCNShape)value; }
		}

		IProposedNetworkEntity IEntityRelationship.To
		{
			get { return Layout.To != null ? collection.GetInstance(Layout.To) : null; }
			set { Layout.To = (BMNCNShape)value; }
		}

		public string DisplayText
		{
			get
			{
				var result = attachment.DisplayText;

				if (IsNonApproved)
				{
					result += " " + Res.GetString("28426165-7d33-4f94-8962-6a6472197385", "(this arrow has not been approved)");
				}
				else if (attachment.BNA_IsDecouple)
				{
					result += " " + Res.GetString("312a1af0-c87e-4d5e-88e9-1c5c42823e33", "(this arrow has been decoupled)");
				}
				else if (attachment.IsRealEntityAttachmentWithNoProcessHeaderLink)
				{
					result += " " + Res.GetString("d3bdd1c6-985a-4f3f-a647-4804557e92dc", "(this arrow does not represent a true dependency)");
				}

				return result;
			}
		}

		#endregion

		#region Business Object Overrides

		protected override ZGuid GetPK()
		{
			return attachment.PK;
		}

		protected override ZString HumanReadableNameCore
		{
			get { return attachment.HumanReadableName; }
		}

		public override bool IsDeleted
		{
			get { return attachment.IsDeleted; }
		}

		#endregion

		#region Validation

		protected override void RunPreSaveValidationCore()
		{
			Validation.ValidateAll();
			base.RunPreSaveValidationCore();
		}

		public NetworkAttachmentValidation Validation
		{
			get { return GetNewValidation(); }
		}

		public NetworkAttachmentValidation GetNewValidation()
		{
			return new NetworkAttachmentValidation(this);
		}

		#endregion
	}
}
