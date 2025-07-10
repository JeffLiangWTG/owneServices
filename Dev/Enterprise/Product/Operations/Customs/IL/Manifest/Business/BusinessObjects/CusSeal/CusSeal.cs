using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.IL.Manifest.Business
{
	public sealed class CusSeal : Customs.Business.CusSeal, IShortSequenceNumberLine
	{
		public CusSeal(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public override void Delete()
		{
			if (!IsDeleted)
			{
				SequenceNumberGenerator?.RecalculateWhenAboutToBeDetachedOrDeleted(this);
			}
			base.Delete();
		}

		[ResourceStringData("382F5B09-E18A-4D6C-8618-7801E5448349", Caption = "Seal Number", MediumCaption = "Seal Number", ShortCaption = "Seal No.")]
		public override ZString BK_SealNumber { get => base.BK_SealNumber; set => base.BK_SealNumber = value; }

		public BusinessObject Parent
		{
			get
			{
				if (fParent == null && !BK_ParentTableCode.IsEmpty && !BK_ParentID.IsEmpty)
				{
					fParent = Factory.Load(BK_ParentTableCode, BK_ParentID);
				}
				return fParent;
			}
		}
		BusinessObject fParent;

		public override ZString BK_ParentTableCode
		{
			get => base.BK_ParentTableCode;
			set
			{
				var oldValue = BK_ParentTableCode;
				base.BK_ParentTableCode = value;

				if (!IsCopying && oldValue != value)
				{
					SequenceNumberGenerator?.RecalculateWhenAdded(this);
				}
			}
		}

		[ResourceStringData("ABC76F1C-363A-4F8E-977F-8F822214BEF3", Caption = "Seal Type", MediumCaption = "Seal Type", ShortCaption = "Seal Type")]
		[List($"{nameof(Lookups)}.{nameof(CusSealLookups.SealTypeList)}")]
		public override ZString BK_SealType { get => base.BK_SealType; set => base.BK_SealType = value; }

		[ResourceStringData("C2E71F96-A275-400C-9EDC-519B1B200228", Caption = "Seal State", MediumCaption = "Seal State", ShortCaption = "Seal State")]
		[List(nameof(Lookups) + "." + nameof(CusSealLookups.UnloadedStates))]
		public override ZString BK_UnloadingState { get => base.BK_UnloadingState; set => base.BK_UnloadingState = value; }

		[ResourceStringData("A6B152F4-3E12-47A4-AC23-82C0F83FF642", Caption = "Seal Party", MediumCaption = "Seal Party", ShortCaption = "Seal Party")]
		[List(nameof(Lookups) + "." + nameof(CusSealLookups.SealingPartyList))]
		public override ZString BK_SealingPartyType { get => base.BK_SealingPartyType; set => base.BK_SealingPartyType = value; }

		public new CusSealLookups Lookups => (CusSealLookups)base.Lookups;

		public new CusSealValidation Validation => (CusSealValidation)base.Validation;

		#region Sequence Number

		ZShort ISequenceNumberLine<ZShort>.SequenceNumber { get => BK_SequenceNumber; set => BK_SequenceNumber = value; }
		ZGuid ISequenceNumberLine.FKToHeader => BK_ParentID;

		ShortSequenceNumberGenerator SequenceNumberGenerator => (Parent as ICusSealSequenceNumberGeneratorProvider)?.SequenceNumberGenerator;

		#endregion

		protected override Customs.Business.CusSealLookups GetNewLookups() => new CusSealLookups(this);

		protected override Customs.Business.CusSealValidation GetNewValidation() => new CusSealValidation(this);
	}
}
