using System.ComponentModel;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.ZArchitecture.Business
{
	[DescriptionProperty(AutoStmEvent.Schema.SE_Desc)]
	public class StmEvent : AutoStmEvent
	{
		public StmEvent(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public static explicit operator Event(StmEvent evt)
		{
			return new Event(evt.SE_Code, evt.SE_DescMultilingual, evt.PK);
		}

		#region Overridden Properties

		[ReadOnly(true)]
		public override ZString SE_Code
		{
			get
			{
				return base.SE_Code;
			}
			set
			{
				base.SE_Code = value;
			}
		}

		[TranslatableDataField(
			Schema.TableName,
			Schema.SE_Desc, @"Database\Odyssey\Data\Public\StmEvent\StmEvent.xml",
			MaxLength = Schema.SE_DescMaxLength,
			Type = typeof(StmEvent),
			SecurityCheckpoint = "EventsEdit",
			Asmid = ResString.AssemblyId)]
		public override ZString SE_Desc
		{
			get
			{
				if (Events.All.Contains(SE_Code))
				{
					if (!SE_IsCustomizable)
					{
						return Events.All[SE_Code].Description;
					}
					else
					{
						return base.SE_Desc;
					}
				}
				else
				{
					return (NoResString)base.SE_Desc;
				}
			}
			set
			{
				base.SE_Desc = value;
			}
		}

		protected bool SE_Desc_ReadOnly
		{
			get
			{
				return !SE_IsCustomizable;
			}
		}

		[ReadOnly(true)]
		public override ZBool SE_IsCustomizable
		{
			get { return base.SE_IsCustomizable; }
			set
			{
				base.SE_IsCustomizable = value;

				SE_DescInfo.RefreshBinding();
			}
		}

		[ReadOnly(true)]
		public override ZString SE_ReferenceFormat
		{
			get
			{
				return base.SE_ReferenceFormat;
			}
			set
			{
				base.SE_ReferenceFormat = value;
			}
		}

		[ReadOnlyMember(nameof(ReferenceFormatOverriddenIsReadOnly))]
		public override ZBool SE_IsRefernceFormatOverridden
		{
			get
			{
				return base.SE_IsRefernceFormatOverridden;
			}
			set
			{
				base.SE_IsRefernceFormatOverridden = value;

				SE_OverriddenReferenceFormatInfo.RefreshBinding();
			}
		}

		public bool ReferenceFormatOverriddenIsReadOnly => SE_Code == Events.BillStatusUpdatedCode;

		protected bool SE_OverriddenReferenceFormat_ReadOnly
		{
			get
			{
				return !SE_IsRefernceFormatOverridden;
			}
		}

		[ReadOnly(true)]
		public override ZBool SE_IsActive
		{
			get { return base.SE_IsActive; }
			set { base.SE_IsActive = value; }
		}

		[ReadOnlyMember(nameof(PropagationSettingsAreReadOnly))]
		public override ZBool SE_PropagateKeyedByEventAndReference
		{
			get => !Events.ChangeLogCodes.Contains(SE_Code) && base.SE_PropagateKeyedByEventAndReference;
			set => base.SE_PropagateKeyedByEventAndReference = value;
		}

		[ReadOnlyMember(nameof(PropagationSettingsAreReadOnly))]
		public override ZBool SE_PropagateToParent
		{
			get => !Events.ChangeLogCodes.Contains(SE_Code) && base.SE_PropagateToParent;
			set => base.SE_PropagateToParent = value;
		}

		public bool PropagationSettingsAreReadOnly => Events.ChangeLogCodes.Contains(SE_Code) || SE_Code == Events.OceanCarrierBookingByTEUCode || SE_Code == Events.BillStatusUpdatedCode || SE_Code == Events.LastFreeDateCode;

		#endregion

		public bool IsInterchangeReady
		{
			get { return SE_Code == Events.InterchangeReady.Code; }
		}

		public bool IsMessageSent
		{
			get
			{
				return
					SE_Code == Events.DeclarationQueued.Code ||
					SE_Code == Events.DeclarationAmendmentQueued.Code ||
					SE_Code == Events.DeclarationCancellationQueued.Code;
			}
		}

		public bool IsMessageAddedToSystem
		{
			get { return (SE_Code == Events.AddedARecordToTheSystem.Code); }
		}

		public MultilingualString SE_DescMultilingual
		{
			get { return GetMultilingual(SE_DescInfo); }
		}

		public override void OnSaving()
		{
			if (IsDeleted || SE_IsActiveInfo.HasChanges)
			{
				Events.ResetInactiveEvents();
			}
			base.OnSaving();
		}
	}
}
