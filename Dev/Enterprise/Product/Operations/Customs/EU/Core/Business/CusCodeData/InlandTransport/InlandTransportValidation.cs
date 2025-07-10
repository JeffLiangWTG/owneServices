using System.Collections.Immutable;
using System.Linq;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.EU.Business.Declaration
{
	public class InlandTransportValidation : Customs.Business.CusCodeDataValidation
	{
		public InlandTransportValidation(InlandTransport parent)
			: base(parent)
		{
		}

		protected new InlandTransport Parent => (InlandTransport)base.Parent;

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateNationality();
		}

		public void ValidateNationality()
		{
			ValidateCalculatedProperty(Parent.NationalityInfo);
		}

		protected void CheckNationality()
		{
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.NationalityInfo);
		}

		protected override void CheckCY_Code()
		{
			base.CheckCY_Code();

			var parent = Parent;
			var code = parent.CY_Code;
			if (code == ExportInlandTransportTypeList.Codes._21 || code == ExportInlandTransportTypeList.Codes._30)
			{
				PropertyIsUniqueInCollectionValidation.CheckPropertyIsUniqueInCollection(parent.CY_CodeInfo);
			}
		}

		protected override void CheckCY_CodeIsNotEmpty()
		{
		}

		protected override void CheckCY_Data()
		{
			base.CheckCY_Data();

			var parent = Parent;
			var targetInfo = parent.CY_DataInfo;
			MandatoryValidation.MessageErrorIfNotEntered(targetInfo);

			if (TransportTypesThatRequireUpperCaseDataOnly.Contains(parent.CY_Code) && parent.CY_Data.ToString().Any(char.IsLower))
			{
				targetInfo.AddMessageError(Res.GetString("14F91D6D-1974-4817-893C-BAFD4C006172", "Transport ID cannot have lower case letters."));
			}
		}

		ImmutableHashSet<string> TransportTypesThatRequireUpperCaseDataOnly => Parent.Factory.GetCachedValue("D224CF70-D2D8-4234-8C8C-A7284F994124",
			() => ImmutableHashSet.Create(
				ExportInlandTransportTypeList.Codes._10,
				ExportInlandTransportTypeList.Codes._20,
				ExportInlandTransportTypeList.Codes._21,
				ExportInlandTransportTypeList.Codes._30,
				ExportInlandTransportTypeList.Codes._31,
				ExportInlandTransportTypeList.Codes._40,
				ExportInlandTransportTypeList.Codes._41,
				ExportInlandTransportTypeList.Codes._80
			)
		);
	}
}
