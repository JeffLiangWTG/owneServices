using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business.CustomValues;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.AR.Manifest.Business
{
	public class AsycudaContainer : ASYCUDA.Business.AsycudaContainer, Integration.Customs.ASYCUDA.ARManifest.IAsycudaContainer
	{
		public AsycudaContainer(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new AsycudaManifestHeader Header => (AsycudaManifestHeader)base.Header;

		public new AsycudaContainerValidation Validation => (AsycudaContainerValidation)base.Validation;

		protected override ManifestBase.AsycudaContainerValidation GetNewValidation() => new AsycudaContainerValidation(this);

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0001:Simplify Names", Justification = "Simplification hides desired base class")]
		public new class Schema : ASYCUDA.Business.AsycudaContainer.Schema
		{
			public const string ACN_ExpireDate = "ACN_ExpireDate";
			public const string ACN_ACEP = "ACN_ACEP";
			public const int ACN_ACEPMaxLength = 20;
		}

		#region ACN_ExpireDate

		[ResourceStringData("AsycudaContainer.ACN_ExpireDate", Caption = "Expire Date")]
		public ZDate ACN_ExpireDate
		{
			get => this.GetSystemDefinedValue<ZDate>(Schema.ACN_ExpireDate);
			set
			{
				var oldValue = ACN_ExpireDate;
				this.SetSystemDefinedValue(Schema.ACN_ExpireDate, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateACN_ExpireDate();
				}
				ACN_ExpireDateInfo.RefreshBinding(oldValue);
			}
		}

		public ZPropertyInfo ACN_ExpireDateInfo => GetZPropertyInfo(Schema.ACN_ExpireDate);

		#endregion

		#region ACN_ACEP

		[MaxLength(Schema.ACN_ACEPMaxLength)]
		[ResourceStringData("AsycudaContainer.ACN_ACEP", Caption = "ACEP Number")]
		public ZString ACN_ACEP
		{
			get => this.GetSystemDefinedValue<ZString>(Schema.ACN_ACEP);
			set
			{
				var oldValue = ACN_ACEP;
				CheckMaximumLength(ACN_ACEPInfo, value);
				this.SetSystemDefinedValue(Schema.ACN_ACEP, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateACN_ACEP();
				}
				ACN_ACEPInfo.RefreshBinding(oldValue);
			}
		}

		public ZPropertyInfo ACN_ACEPInfo => GetZPropertyInfo(Schema.ACN_ACEP);

		#endregion
	}
}
