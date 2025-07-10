using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class QuarantineSupportingInfo : CusSupportingInfo
	{
		public QuarantineSupportingInfo(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#pragma warning disable IDE0001 // Prevent simplification to base class
		public new class Schema : CusSupportingInfo.Schema
		#pragma warning restore IDE0001 // Prevent simplification to base class
		{
			public const string Description = "Description";
		}

		[List(nameof(Lookups) + "." + nameof(QuarantineSupportingInfoLookups.CSIDescriptionList))]
		[ResourceStringData("174A8640-2D5D-4FE4-8985-7614ED84B62C", Caption = "Code", FullDescription = "Declaration Codes entered into the grid will be sent in the message.")]
		public override ZString CSI_Description
		{
			get => base.CSI_Description;
			set
			{
				var hasChange = CSI_Description != value;
				base.CSI_Description = value;
				if (hasChange)
				{
					DescriptionInfo.RefreshBinding();
				}
			}
		}

		[ResourceStringData("8f6ea208-b81c-4cc2-9a41-05f3ce75b950", Caption = "Description")]
		public ZString Description
		{
			get
			{
				return Lookups.DeclarationCodeList.GetDescriptionFromCode(CSI_Description);
			}
		}

		public ZPropertyInfo DescriptionInfo
		{
			get { return GetZPropertyInfo(Schema.Description); }
		}

		public new QuarantineSupportingInfoLookups Lookups => (QuarantineSupportingInfoLookups)base.Lookups;
		protected override CusSupportingInfoValidation GetNewValidation() => new QuarantineSupportingInfoValidation(this);

		protected override CusSupportingInfoLookups GetNewLookups() => new QuarantineSupportingInfoLookups(this);
	}
}
