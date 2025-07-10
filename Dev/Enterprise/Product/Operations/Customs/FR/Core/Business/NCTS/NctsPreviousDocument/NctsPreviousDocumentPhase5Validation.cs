using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.FR.Business.CusTempStorage;

namespace Enterprise.Customs.FR.Business.NCTS
{
	public class NctsPreviousDocumentPhase5Validation : EU.NCTS.Business.NctsPreviousDocumentPhase5Validation
	{
		public NctsPreviousDocumentPhase5Validation(NctsPreviousDocument parent)
			: base(parent)
		{ }

		protected new NctsPreviousDocument Parent => (NctsPreviousDocument)base.Parent;

		protected override void CheckCSI_ReferenceNumber()
		{
			base.CheckCSI_ReferenceNumber();
			var parent = Parent;
			MandatoryValidation.MessageErrorIfNotEntered(parent.CSI_ReferenceNumberInfo);

			if (IsUsedForTemporaryStorage(parent) && !parent.CSI_ReferenceNumber.IsEmpty && ISTRegHeader is null)
			{
				parent.CSI_ReferenceNumberInfo.AddMessageError(Res.GetString("A9F5C91-493A-4873-A7F5-211B407C44C6", "The entered IST reference {0} does not match any Temporary Storage Register.", parent.CSI_ReferenceNumber));
			}
		}

		protected override void CheckCSI_ItemNumber()
		{
			base.CheckCSI_ItemNumber();
			var parent = Parent;

			if (!parent.CSI_ReferenceNumber.IsEmpty && IsUsedForTemporaryStorage(parent) && ISTRegHeader is not null && RegisterMatchingLine is null)
			{
				parent.CSI_ItemNumberInfo.AddMessageError(Res.GetString("F87A9FA8-7B89-48F9-A3CD-DB36DD03C93D", "The entered line number does not exist for this IST."));
			}
		}

		protected override void CheckCSI_UnitOfQuantity2()
		{
			base.CheckCSI_UnitOfQuantity2();
			var parent = Parent;

			if (IsUsedForTemporaryStorage(parent) && RegisterMatchingLine is CusTempStorageRegLine matchingLine && matchingLine.SRL_PackageType != parent.CSI_UnitOfQuantity2)
			{
				parent.CSI_UnitOfQuantity2Info.AddMessageError(Res.GetString("F87A9FA8-7B89-48F9-A3CD-DB36DD03C93E", "The entered package type does not equal the package type for this line on the IST."));
			}
		}

		protected override void CheckCSI_Quantity2()
		{
			base.CheckCSI_Quantity2();
			var parent = Parent;

			if (IsUsedForTemporaryStorage(parent) && RegisterMatchingLine is CusTempStorageRegLine matchingLine && matchingLine.SRL_PackagesRemaining < parent.CSI_Quantity2)
			{
				parent.CSI_Quantity2Info.AddMessageError(Res.GetString("F87A9FA8-7B89-48F9-A3CD-DB36DD03C93F", "The entered package quantity should be less than or equal to the corresponding line in the IST."));
			}
		}

		protected override void CheckCSI_Quantity()
		{
			base.CheckCSI_Quantity();
			var parent = Parent;

			if (IsUsedForTemporaryStorage(parent) && RegisterMatchingLine is CusTempStorageRegLine matchingLine && matchingLine.GrossWeightRemainingCalculated < parent.CSI_Quantity)
			{
				parent.CSI_QuantityInfo.AddMessageError(Res.GetString("F87A9FA8-7B89-48F9-A3CD-DB36DD03C93G", "The entered quantity should be less than or equal to the corresponding line in the IST."));
			}
		}

		protected override void CheckCSI_UnitOfQuantity()
		{
			base.CheckCSI_UnitOfQuantity();
			var parent = Parent;

			if (IsUsedForTemporaryStorage(parent) && RegisterMatchingLine is CusTempStorageRegLine matchingLine && matchingLine.SRL_GrossWeightUQ != parent.CSI_UnitOfQuantity)
			{
				parent.CSI_UnitOfQuantityInfo.AddMessageError(Res.GetString("B42AF78A-777F-4139-BBB3-37AFE307A53D", "The entered measurement unit does not equal the unit for this line on the IST."));
			}
		}

		protected override void CheckCSI_SubType()
		{
		}

		CusTempStorageRegHeader ISTRegHeader
		{
			get
			{
				var parent = Parent;
				return parent.Factory.GetCachedValue($"FR.TempStorageRegister:{parent.CSI_ReferenceNumber}", () =>
				{
					return GetPreviousISTHeader()?.RegisterHeader;
				});
			}
		}

		static bool IsUsedForTemporaryStorage(NctsPreviousDocument parent) => parent.CSI_Code == UniversalReferenceConstants.RefCusCodeList.PreviousDocumentsCodes.TemporayStorage;

		CusTempStorageRegLine RegisterMatchingLine => ISTRegHeader?.CusTempStorageRegLines.Cast<CusTempStorageRegLine>().FirstOrDefault(x => x.SRL_LineNumber == Parent.CSI_ItemNumber);

		CusTempStorageJobHeader GetPreviousISTHeader() => ComplementaryJobISTFinder.FindFromReferenceNumber(Parent.Factory, Parent.CSI_ReferenceNumber, Parent.Declaration?.CountryCode ?? Core.Constants.CountryCodes.France);
	}
}
