using System.Collections.Generic;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class TCPGAHeaderAddInfoValidationTest : BusinessObjectValidationTestCase
	{
		public void TestTestCheckCA_SubProgram()
		{
			header.AddInfoValidation.ValidateCA_SubProgram();
			AssertNoMessageErrorContaining(header.CA_SubProgramInfo, MandatoryValidation.YouHaveNotEntered);

			header.CA_VPRProgramInd = YesNoList.Codes.Yes;
			header.AddInfoValidation.ValidateCA_SubProgram();
			AssertHasMessageErrorContaining(header.CA_SubProgramInfo, MandatoryValidation.YouHaveNotEntered);

			header.CA_SubProgram = TCPGAVehicleProgramCodes.Codes.VVP;
			AssertNoMessageErrorContaining(header.CA_SubProgramInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckCA_ProductClass()
		{
			var availabePrograms = new[]
			{
				TCPGADepartmentCodes.Codes.TPR,
				TCPGAVehicleProgramCodes.Codes.PIL,
				TCPGAVehicleProgramCodes.Codes.VCC,
				TCPGAVehicleProgramCodes.Codes.VFS,
				TCPGAVehicleProgramCodes.Codes.VFC,
				TCPGAVehicleProgramCodes.Codes.VAE,
				TCPGAVehicleProgramCodes.Codes.VCR,
				TCPGAVehicleProgramCodes.Codes.VVP
			};

			AssertMandatoryProperty(header.CA_ProductClassInfo, availabePrograms);
			AssertListValidationProperty(header.CA_ProductClassInfo, availabePrograms);
		}

		public void TestCheckCA_ProductType()
		{
			var availabePrograms = new[]
			{
				TCPGADepartmentCodes.Codes.TPR
			};

			AssertMandatoryProperty(header.CA_ProductTypeInfo, availabePrograms);
			AssertListValidationProperty(header.CA_ProductTypeInfo, availabePrograms);
		}

		public void TestCheckCA_ProductSize()
		{
			var availabePrograms = new[]
			{
				TCPGADepartmentCodes.Codes.TPR
			};

			AssertMandatoryProperty(header.CA_ProductSizeInfo, availabePrograms);
			AssertListValidationProperty(header.CA_ProductSizeInfo, availabePrograms);
		}

		public void TestCheckCA_ImportReasonCode()
		{
			var availabePrograms = new[]
			{
				TCPGADepartmentCodes.Codes.TPR
			};

			AssertMandatoryProperty(header.CA_ImportReasonCodeInfo, availabePrograms);
			AssertListValidationProperty(header.CA_ImportReasonCodeInfo, availabePrograms);
		}

		public void TestCA_VehicleStatus()
		{
			var availabePrograms = new[]
			{
				TCPGAVehicleProgramCodes.Codes.VVP
			};

			AssertMandatoryProperty(header.CA_VehicleStatusInfo, availabePrograms);
			AssertListValidationProperty(header.CA_VehicleStatusInfo, availabePrograms);
		}

		public void TestCheckCA_VehicleCondition()
		{
			var availabePrograms = new[]
			{
				TCPGAVehicleProgramCodes.Codes.VCC,
				TCPGAVehicleProgramCodes.Codes.VFS,
				TCPGAVehicleProgramCodes.Codes.VFC,
				TCPGAVehicleProgramCodes.Codes.VAE,
				TCPGAVehicleProgramCodes.Codes.VCR,
			};

			AssertMandatoryProperty(header.CA_VehicleConditionInfo, availabePrograms);
			AssertListValidationProperty(header.CA_VehicleConditionInfo, availabePrograms);
		}

		public void TestCheckCA_ODOReading()
		{
			invoiceLine.JI_CountryOfOrigin = "MX";
			var tcpgaHeader = invoiceLine.TCPGAHeader;
			tcpgaHeader.CA_VPRProgramInd = "Y";

			tcpgaHeader.CA_SubProgram = TCPGAVehicleProgramCodes.Codes.VFS;
			tcpgaHeader.AddInfoValidation.ValidateCA_ODOReading();
			AssertHasMessageErrorContaining(header.CA_ODOReadingInfo, MandatoryValidation.YouHaveNotEntered);

			tcpgaHeader.CA_ODOReading = "AAA";
			AssertNoMessageErrorContaining(header.CA_ODOReadingInfo, MandatoryValidation.YouHaveNotEntered);

			tcpgaHeader.CA_ODOReading = string.Empty;
			tcpgaHeader.CA_SubProgram = TCPGAVehicleProgramCodes.Codes.VFC;
			tcpgaHeader.AddInfoValidation.ValidateCA_ODOReading();
			AssertHasMessageErrorContaining(header.CA_ODOReadingInfo, MandatoryValidation.YouHaveNotEntered);

			tcpgaHeader.CA_ODOReading = "AAA";
			AssertNoMessageErrorContaining(header.CA_ODOReadingInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCA_ChassisYear()
		{
			var availabePrograms = new[]
			{
				TCPGAVehicleProgramCodes.Codes.PIL,
				TCPGAVehicleProgramCodes.Codes.VCC,
				TCPGAVehicleProgramCodes.Codes.VFS,
				TCPGAVehicleProgramCodes.Codes.VFC,
				TCPGAVehicleProgramCodes.Codes.VAE,
				TCPGAVehicleProgramCodes.Codes.VCR,
				TCPGAVehicleProgramCodes.Codes.VUV
			};

			AssertListValidationProperty(header.CA_ChassisYearInfo, availabePrograms);
		}

		public void TestCheckCA_ManufactureYear()
		{
			var availabePrograms = new[]
			{
				TCPGAVehicleProgramCodes.Codes.PIL,
				TCPGAVehicleProgramCodes.Codes.VCC,
				TCPGAVehicleProgramCodes.Codes.VFS,
				TCPGAVehicleProgramCodes.Codes.VFC,
				TCPGAVehicleProgramCodes.Codes.VAE,
				TCPGAVehicleProgramCodes.Codes.VCR,
				TCPGAVehicleProgramCodes.Codes.VUV,
				TCPGAVehicleProgramCodes.Codes.VVP
			};

			AssertMandatoryProperty(header.CA_ManufactureYearInfo, availabePrograms);
			AssertListValidationProperty(header.CA_ManufactureYearInfo, availabePrograms);
		}

		public void TestCheckCA_ManufactureMonth()
		{
			var availabePrograms = new[]
			{
				TCPGAVehicleProgramCodes.Codes.PIL,
				TCPGAVehicleProgramCodes.Codes.VCC,
				TCPGAVehicleProgramCodes.Codes.VFS,
				TCPGAVehicleProgramCodes.Codes.VFC,
				TCPGAVehicleProgramCodes.Codes.VAE,
				TCPGAVehicleProgramCodes.Codes.VCR,
				TCPGAVehicleProgramCodes.Codes.VUV,
				TCPGAVehicleProgramCodes.Codes.VVP
			};

			AssertMandatoryProperty(header.CA_ManufactureMonthInfo, availabePrograms);
			AssertListValidationProperty(header.CA_ManufactureMonthInfo, availabePrograms);
		}

		public void TestCheckCA_CriteriaConformance()
		{
			var availabePrograms = new[]
			{
				TCPGAVehicleProgramCodes.Codes.PIL,
				TCPGAVehicleProgramCodes.Codes.VCC,
				TCPGAVehicleProgramCodes.Codes.VFS,
				TCPGAVehicleProgramCodes.Codes.VFC,
				TCPGAVehicleProgramCodes.Codes.VAE,
				TCPGAVehicleProgramCodes.Codes.VCR
			};

			AssertMandatoryProperty(header.CA_CriteriaConformanceInfo, availabePrograms);
			AssertListValidationProperty(header.CA_CriteriaConformanceInfo, availabePrograms);
		}

		public void TestCheckCA_TitleStatus()
		{
			var availabePrograms = new[]
			{
				TCPGAVehicleProgramCodes.Codes.VFS,
				TCPGAVehicleProgramCodes.Codes.VVP
			};

			AssertMandatoryProperty(header.CA_TitleStatusInfo, availabePrograms);
			AssertListValidationProperty(header.CA_TitleStatusInfo, availabePrograms);
		}

		public void TestCA_SubProgram()
		{
			header.CA_VPRProgramInd = YesNoList.Codes.Yes;
			header.AddInfoValidation.ValidateCA_SubProgram();
			AssertHasMessageErrorContaining(header.CA_SubProgramInfo, MandatoryValidation.YouHaveNotEntered);

			header.CA_SubProgram = "~";
			AssertHasMessageError(header.CA_SubProgramInfo, ListValidation.InvalidCodeMessageError);
			header.CA_SubProgram = TCPGAVehicleProgramCodes.Codes.PIL;
			AssertNoMessageError(header.CA_SubProgramInfo, ListValidation.InvalidCodeMessageError);
		}

		void AssertMandatoryProperty(ZPropertyInfo propertyInfo, IEnumerable<string> availablePrograms, string messageError = "You have not entered")
		{
			CombineAssertions(() =>
			{
				foreach (var programCode in allProgramCodes)
				{
					if (programCode == TCPGADepartmentCodes.Codes.TPR)
					{
						header.CA_TPRProgramInd = YesNoList.Codes.Yes;
					}
					else
					{
						header.CA_VPRProgramInd = YesNoList.Codes.Yes;
						header.CA_SubProgram = programCode;
					}

					header.AddInfoValidation.ValidateAll();
					AssertEquals($"propertyInfo:{propertyInfo.Name}, programCode:{programCode}, isMandatoryAvailabe:{availablePrograms.Contains(programCode)}"
						, availablePrograms.Contains(programCode)
						, propertyInfo.Notifications.GetMessageErrors().ContainsNotificationContaining(messageError));

					if (programCode == TCPGADepartmentCodes.Codes.TPR)
					{
						header.CA_TPRProgramInd = YesNoList.Codes.No;
					}
					else
					{
						header.CA_VPRProgramInd = YesNoList.Codes.No;
						header.CA_SubProgram = string.Empty;
					}
				}
			});
		}

		void AssertListValidationProperty(ZPropertyInfo propertyInfo, IEnumerable<string> availablePrograms)
		{
			CombineAssertions(() =>
			{
				foreach (var programCode in header.AddInfoLookups.ProgramCodesList.GetAllCodes())
				{
					if (programCode == TCPGADepartmentCodes.Codes.TPR)
					{
						header.CA_TPRProgramInd = YesNoList.Codes.Yes;
					}
					else
					{
						header.CA_VPRProgramInd = YesNoList.Codes.Yes;
						header.CA_SubProgram = programCode;
					}

					propertyInfo.Value = (ZString)"~";

					header.AddInfoValidation.ValidateAll();
					AssertEquals($"propertyInfo:{propertyInfo.Name}, programCode:{programCode}, isListAvailabe:{availablePrograms.Contains(programCode)}"
						, availablePrograms.Contains(programCode)
						, propertyInfo.Notifications.GetMessageErrors().ContainsNotificationContaining(ListValidation.InvalidCodeMessageError.ToString()));

					if (programCode == TCPGADepartmentCodes.Codes.TPR)
					{
						header.CA_TPRProgramInd = YesNoList.Codes.No;
					}
					else
					{
						header.CA_VPRProgramInd = YesNoList.Codes.No;
						header.CA_SubProgram = string.Empty;
					}
				}
			});
		}

		readonly string[] allProgramCodes = new[]
		{
			TCPGADepartmentCodes.Codes.TPR,
			TCPGAVehicleProgramCodes.Codes.PIG,
			TCPGAVehicleProgramCodes.Codes.PIL,
			TCPGAVehicleProgramCodes.Codes.VCC,
			TCPGAVehicleProgramCodes.Codes.VFS,
			TCPGAVehicleProgramCodes.Codes.VFC,
			TCPGAVehicleProgramCodes.Codes.VAE,
			TCPGAVehicleProgramCodes.Codes.VCR,
			TCPGAVehicleProgramCodes.Codes.VUV,
			TCPGAVehicleProgramCodes.Codes.VVP
		};

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.IID;
			invoice = declaration.Invoices.AddNew();
			invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.CA_TCInd = YesNoList.Codes.Yes;
			header = invoiceLine.TCPGAHeader;
		}
		TCPGAHeader header;
		JobDeclaration declaration;
		JobComInvoiceHeader invoice;
		JobComInvoiceLine invoiceLine;

		#endregion
	}
}
