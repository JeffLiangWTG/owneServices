using System;
using System.ComponentModel;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.EU.Business.Declaration
{
	public partial class CusContainer : AutoCusContainer, ICusSealSequenceNumberGeneratorProvider, ICusSealTypeSupporter
	{
		public CusContainer(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		[ReadOnly(true)]
		[List(nameof(MessageStatusList))]
		public override ZString CO_MessageStatus
		{
			get { return base.CO_MessageStatus; }
			set { base.CO_MessageStatus = value; }
		}

		public ZString CO_MessageStatusExplanation
		{
			get { return MessageStatusList.GetDescriptionFromCode(CO_MessageStatus); }
		}

		public ZPropertyInfo CO_MessageStatusExplanationInfo
		{
			get { return GetZPropertyInfo(Schema.CO_MessageStatusExplanation); }
		}

		public CodeDescriptionPairList MessageStatusList
		{
			get { return new ContainerStatusCodesList(); }
		}

		public override ZString CO_Seal
		{
			get => base.CO_Seal;
			set
			{
				var oldValue = base.CO_Seal;
				base.CO_Seal = value;
				if (oldValue != CO_Seal && !IsCopying)
				{
					UpdateAdditionalSealsReadOnly(value, CO_SecondSeal);
				}
			}
		}

		[ResourceStringData("D74BF73F-4621-4C43-8E87-47914865B608", FullDescription = "Second Seal Number", Caption = "Second Seal No.", MediumCaption = "Second Seal", ShortCaption = "2nd Seal")]
		public override ZString CO_SecondSeal
		{
			get => base.CO_SecondSeal;
			set
			{
				var oldValue = base.CO_SecondSeal;
				base.CO_SecondSeal = value;
				if (oldValue != CO_SecondSeal && !IsCopying)
				{
					UpdateAdditionalSealsReadOnly(CO_Seal, value);
				}
			}
		}

		[ResourceStringData("C2B4A750-8F3C-4802-9780-F8365F617581", Caption = "Control")]
		[ReadOnly(true)]
		public override ZBool ZG_IsControl { get => base.ZG_IsControl; set => base.ZG_IsControl = value; }

		[ResourceStringData("58ACDF5E-5E27-4062-BD04-1EC6F5B995BC", Caption = "Unloaded")]
		[ReadOnly(true)]
		public override ZBool ZG_IsUnloaded { get => base.ZG_IsUnloaded; set => base.ZG_IsUnloaded = value; }

		public new class Schema : AutoCusContainer.Schema
		{
			public const string CO_MessageStatusExplanation = "CO_MessageStatusExplanation";
		}

		protected override bool ShouldDefaultPackingInformation
		{
			get
			{
				if (Declaration is JobDeclaration declaration && declaration.EquipmentsRequired)
				{
					return declaration.CusContainers.Count == 1 && declaration.Equipments.Count == 0 && declaration.CusContainers.Contains(this);
				}

				return true;
			}
		}

		public override void Delete()
		{
			AdditionalSeals.DeleteAll();
			base.Delete();
		}

		[ChildEditable]
		public CusSealCollection AdditionalSeals
		{
			get
			{
				if (additionalSeals == null)
				{
					additionalSeals = new CusSealCollection(this);
					RegisterEditableChildObject(additionalSeals);
					UpdateAdditionalSealsReadOnly(CO_Seal, CO_SecondSeal);
				}
				return additionalSeals;
			}
		}
		CusSealCollection additionalSeals;

		Type ICusSealTypeSupporter.CusSealType => typeof(CusSeal);

		ShortSequenceNumberGenerator sealsSequenceNumberGeneratorCache;
		internal ShortSequenceNumberGenerator SealsSequenceNumberGenerator => sealsSequenceNumberGeneratorCache ?? (sealsSequenceNumberGeneratorCache = new ShortSequenceNumberGenerator(() => AdditionalSeals));

		ShortSequenceNumberGenerator ICusSealSequenceNumberGeneratorProvider.SequenceNumberGenerator => SealsSequenceNumberGenerator;

		void UpdateAdditionalSealsReadOnly(ZString seal1, ZString seal2)
		{
			var newReadOnlyValue = seal1.IsEmpty || seal2.IsEmpty;
			if (AdditionalSeals.ReadOnly != newReadOnlyValue)
			{
				AdditionalSeals.SetReadOnlyIncludingChildren(newReadOnlyValue);
			}
		}
	}
}
