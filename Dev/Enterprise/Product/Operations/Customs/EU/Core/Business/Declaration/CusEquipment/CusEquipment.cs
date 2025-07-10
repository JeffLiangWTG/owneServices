using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.EU.Business.Declaration
{
	public class CusEquipment : Customs.Business.CusEquipment, Integration.Customs.EU.ICusEquipment, ICusSealTypeSupporter, ICusSealSequenceNumberGeneratorProvider
	{
		public CusEquipment(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		[ResourceStringData("EB366CFB-1995-46C3-8591-8968D33A7012", Caption = "Identification Number",
			MediumCaption = "ID Number", ShortCaption = "ID")]
		public override ZString CEQ_IdentificationNumber
		{
			get => base.CEQ_IdentificationNumber;
			set
			{
				var originalContainerNumber = CEQ_IdentificationNumber;
				base.CEQ_IdentificationNumber = value;
				if (!IsCopying && originalContainerNumber != CEQ_IdentificationNumber)
				{
					if (originalContainerNumber.IsEmpty && Declaration is JobDeclaration declaration
						&& declaration.Equipments.Count == 1 && declaration.CusContainers.Count == 0
						&& declaration.Equipments.Contains(this) && declaration.EquipmentsRequired)
					{
						DefaultPackingInformationIfNeeded();
					}
				}
			}
		}

		#region DefaultPackingInformationIfNeeded
		void DefaultPackingInformationIfNeeded()
		{
			if (Declaration is JobDeclaration declaration)
			{
				foreach (PackingGroup packGroup in declaration.PackingGroups)
				{
					packGroup.CR_CEQ_Equipment = PK;
				}
			}
		}

		#endregion

		public new JobDeclaration Declaration => (JobDeclaration)base.Declaration;

		public new CusEquipmentValidation Validation => (CusEquipmentValidation)base.Validation;

		protected override Customs.Business.CusEquipmentValidation GetNewValidation() => new CusEquipmentValidation(this);

		public override void OnSaving()
		{
			if (!IsDeleted && CEQ_IdentificationNumber.IsEmpty)
			{
				Delete();
			}
			base.OnSaving();
		}

		public override void Delete()
		{
			Seals.DeleteAll();
			base.Delete();
		}

		[ChildEditable]
		public CusSealCollection Seals
		{
			get
			{
				if (seals == null)
				{
					seals = CreateSealsCore();
					RegisterEditableChildObject(seals);
				}
				return seals;
			}
		}
		CusSealCollection seals;

		Type ICusSealTypeSupporter.CusSealType => typeof(CusSeal);

		#region Seals

		ShortSequenceNumberGenerator sealsSequenceNumberGeneratorCache;
		internal ShortSequenceNumberGenerator SealsSequenceNumberGenerator => sealsSequenceNumberGeneratorCache ?? (sealsSequenceNumberGeneratorCache = new ShortSequenceNumberGenerator(() => Seals));

		ShortSequenceNumberGenerator ICusSealSequenceNumberGeneratorProvider.SequenceNumberGenerator => SealsSequenceNumberGenerator;

		protected virtual CusSealCollection CreateSealsCore()
		{
			return new CusSealCollection(this);
		}

		#endregion
	}
}
