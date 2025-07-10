using System;
using System.Collections.ObjectModel;
using CargoWise.Customs.IE.MessageDefinitions;
using CargoWise.Customs.IE.MessageDefinitions.AISVersion1_0.AIS_complex;
using CargoWise.Customs.IE.MessageDefinitions.AISVersion1_0.TS304;
using CargoWise.Customs.IE.MessageDefinitions.AISVersion1_0.TS305;
using CargoWise.Customs.IE.MessageDefinitions.AISVersion1_0.TS309;
using CargoWise.Customs.IE.MessageDefinitions.AISVersion1_0.TS315V;
using CargoWise.Customs.IE.MessageDefinitions.AISVersion1_0.TS316;
using CargoWise.Customs.IE.MessageDefinitions.AISVersion1_0.TS328;
using CargoWise.Customs.IE.MessageDefinitions.AISVersion1_0.TS333;
using CargoWise.Customs.IE.MessageDefinitions.AISVersion1_0.TS351;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.IE.Messaging.UCC5.Testing
{
	public static class AISUCC5InterchangeProcessorTestHelper
	{
		const string DateFormat = "yyyyMMdd";
		const string DateTimeFormat = "yyyyMMddHHmmZZZ";

		public static void CreateFunctionalErrorTypeReferenceTestData(BusinessObjectFactory factory)
		{
			var dataGrouping = Core.Constants.Customs.Universal.RefDataGrouping.Codes.IEUcc5;
			var codeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ErrorCode;

			var helper = new UniversalReferenceTestDataHelper(factory);
			helper.CreateNewOrGetExistingCusCodeType(codeType, "Error Codes", dataGrouping);
			helper.CreateNewOrGetExistingCusCodeList(dataGrouping, codeType, "13", "Missing value", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList(dataGrouping, codeType, "40", "Element too short", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			factory.Save();
		}

		public static Collection<FunctionalErrorType> CreateFunctionalErrorTypeObjects() => new Collection<FunctionalErrorType>
		{
			new FunctionalErrorType
			{
				ErrorPointer = "ErrorPointer001",
				ErrorType = "13",
				ErrorReason = "ER1",
				ErrorMessage = "Functional Error Message 1",
				OriginalAttributeValue = "Original Attribute Value 1",
			},
			new FunctionalErrorType
			{
				ErrorPointer = "ErrorPointer002",
				ErrorType = "40",
				ErrorReason = "ER2",
				ErrorMessage = "Functional Error Message 2",
				OriginalAttributeValue = "Original Attribute Value 2",
			},
		};

		public const string ExpectedFunctionalErrorInterpretation = @"Functional Error: 1<br />
			<br />
			<table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table"">
				<tr><td>Error Reason</td><td>ER1</td></tr>
				<tr><td>Error Type</td><td>13</td></tr>
				<tr><td>Error Type Description</td><td>Missing value</td></tr>
				<tr><td>Error Message</td><td>Functional Error Message 1</td></tr>
				<tr><td>Original Attribute Value</td><td>Original Attribute Value 1</td></tr>
				<tr><td>Error Pointer</td><td>ErrorPointer001</td></tr>
			</table><br />
			<br />
			Functional Error: 2<br />
			<br />
			<table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table"">
				<tr><td>Error Reason</td><td>ER2</td></tr>
				<tr><td>Error Type</td><td>40</td></tr>
				<tr><td>Error Type Description</td><td>Element too short</td></tr>
				<tr><td>Error Message</td><td>Functional Error Message 2</td></tr>
				<tr><td>Original Attribute Value</td><td>Original Attribute Value 2</td></tr>
				<tr><td>Error Pointer</td><td>ErrorPointer002</td></tr>
			</table>";

		public static string GetStandardUCC5IM433Text()
		{
			var data = new CargoWise.Customs.IE.MessageDefinitions.AISVersion1_0.IM433.Im433()
			{
				Declaration = new CargoWise.Customs.IE.MessageDefinitions.AISVersion1_0.IM433.DeclarationType()
				{
					Mrn = "12MRN345CDEFG678R9",
					RejectionDate = "202402292359UTC",
					RejectionReason = "Rejection Reason",
					CustomsOffices = new CargoWise.Customs.IE.MessageDefinitions.AISVersion1_0.IM433.DeclarationTypeCustomsOffices()
					{
						CustomsOfficeLodgement = "ABCDE123",
					},
					Parties = new CargoWise.Customs.IE.MessageDefinitions.AISVersion1_0.IM433.DeclarationTypeParties()
					{
						Declarant = new CargoWise.Customs.IE.MessageDefinitions.AISVersion1_0.AIS_complex.DeclarantType()
						{
							Declarant318 = "TOM THE DECLARANT",
							Declarant317 = new TraderType()
							{
								Name = "BOB THE BUILDER",
								StreetAndNumber = "Grand Canal Street Upper 1",
								CountryCode = "IE",
								Postcode = "D04 Y7R5",
								City = "Dublin",
							}
						}
					},
				},
				FunctionalError = CreateFunctionalErrorTypeObjects(),
			};
			return IEXmlObjectSerializer.Serialize(data);
		}

		public static Ts304 CreateTS304Object(string mrn, DateTime acceptanceDate, string remarks)
		{
			return new Ts304()
			{
				Declaration = new CargoWise.Customs.IE.MessageDefinitions.AISVersion1_0.TS304.DeclarationType()
				{
					Mrn = mrn,
					AmendmentAcceptanceDate = acceptanceDate.ToString(DateFormat),
					CustomsOffices = new CargoWise.Customs.IE.MessageDefinitions.AISVersion1_0.TS304.DeclarationTypeCustomsOffices()
					{
						CustomsOfficeLodgement = "IE123456"
					},
					Parties = new CargoWise.Customs.IE.MessageDefinitions.AISVersion1_0.TS304.DeclarationTypeParties()
					{
						Declarant = new CargoWise.Customs.IE.MessageDefinitions.AISVersion1_0.TS304.DeclarantType()
						{
							Declarant318 = "IE123890"
						}
					},
					Remarks = remarks,
				}
			};
		}

		public static string GetTS304Text(string mrn, DateTime acceptanceDate, string remarks) => IEXmlObjectSerializer.Serialize(CreateTS304Object(mrn, acceptanceDate, remarks));

		public static Ts305 CreateTS305Object(string mrn, DateTime rejectionDate, string rejectionReason)
		{
			return new Ts305()
			{
				Declaration = new CargoWise.Customs.IE.MessageDefinitions.AISVersion1_0.TS305.DeclarationType()
				{
					Mrn = mrn,
					AmendmentRejectionDate = rejectionDate.ToString(DateFormat),
					AmendmentRejectionReason = rejectionReason,
					CustomsOffices = new CargoWise.Customs.IE.MessageDefinitions.AISVersion1_0.TS305.DeclarationTypeCustomsOffices()
					{
						CustomsOfficeLodgement = "IE123456"
					},
					Parties = new CargoWise.Customs.IE.MessageDefinitions.AISVersion1_0.TS305.DeclarationTypeParties()
					{
						Declarant = new CargoWise.Customs.IE.MessageDefinitions.AISVersion1_0.TS305.DeclarantType()
						{
							Declarant318 = "IE123890"
						}
					},
				},
				FunctionalError = CreateFunctionalErrorTypeObjects(),
			};
		}

		public static string GetTS305Text(string mrn, DateTime rejectionDate, string rejectionReason) => IEXmlObjectSerializer.Serialize(CreateTS305Object(mrn, rejectionDate, rejectionReason));

		public static Ts309 CreateTS309Object(string mrn, DateTime invalidationDate)
		{
			return new Ts309()
			{
				Declaration = new CargoWise.Customs.IE.MessageDefinitions.AISVersion1_0.TS309.DeclarationType()
				{
					Mrn = mrn,
					InvalidationDecision = true,
					InvalidationInitiatedByCustoms = false,
					InvalidationJustification = "justification",
					DateOfInvalidationDecision = invalidationDate.ToString(DateFormat),
					DateOfInvalidationRequest = invalidationDate.ToString(DateFormat),
					DateOfInvalidation = invalidationDate.ToString(DateFormat),
					CustomsOffices = new CargoWise.Customs.IE.MessageDefinitions.AISVersion1_0.TS309.DeclarationTypeCustomsOffices()
					{
						CustomsOfficeLodgement = "IE123456"
					},
					Parties = new CargoWise.Customs.IE.MessageDefinitions.AISVersion1_0.TS309.DeclarationTypeParties()
					{
						Declarant = new CargoWise.Customs.IE.MessageDefinitions.AISVersion1_0.TS309.DeclarantType()
						{
							Declarant318 = "IE123890"
						}
					},
				},
				FunctionalError = CreateFunctionalErrorTypeObjects(),
			};
		}

		public static string GetTS309Text(string mrn, DateTime invalidationDate) => IEXmlObjectSerializer.Serialize(CreateTS309Object(mrn, invalidationDate));

		public static Ts315V CreateTS315VObject(string mrn, string lrn, DateTime acknowledgementDate)
		{
			return new Ts315V()
			{
				Declaration = new CargoWise.Customs.IE.MessageDefinitions.AISVersion1_0.TS315V.DeclarationType()
				{
					Mrn = mrn,
					Lrn25 = lrn,
					DeclarationAcknowledgementDate = acknowledgementDate.ToString(DateFormat),
					CustomsOffices = new CargoWise.Customs.IE.MessageDefinitions.AISVersion1_0.TS315V.DeclarationTypeCustomsOffices()
					{
						CustomsOfficeLodgement = "IE123456"
					},
					Parties = new CargoWise.Customs.IE.MessageDefinitions.AISVersion1_0.TS315V.DeclarationTypeParties()
					{
						Declarant = new CargoWise.Customs.IE.MessageDefinitions.AISVersion1_0.AIS_complex.DeclarantType()
						{
							Declarant318 = "IE123890"
						}
					},
				}
			};
		}

		public static string GetTS315VText(string mrn, string lrn, DateTime acknowledgementDate) => IEXmlObjectSerializer.Serialize(CreateTS315VObject(mrn, lrn, acknowledgementDate));

		public static Ts316 CreateTS316Object(string lrn, DateTime rejectionDate)
		{
			return new Ts316()
			{
				Declaration = new CargoWise.Customs.IE.MessageDefinitions.AISVersion1_0.TS316.DeclarationType()
				{
					Lrn25 = lrn,
					RejectionDate = rejectionDate.ToString(DateFormat),
					RejectionMotivationText = "reason",
					CustomsOffices = new CargoWise.Customs.IE.MessageDefinitions.AISVersion1_0.TS316.DeclarationTypeCustomsOffices()
					{
						CustomsOfficeLodgement = "IE123456"
					},
					Parties = new CargoWise.Customs.IE.MessageDefinitions.AISVersion1_0.TS316.DeclarationTypeParties()
					{
						Declarant = new CargoWise.Customs.IE.MessageDefinitions.AISVersion1_0.TS316.DeclarantType()
						{
							Declarant318 = "IE123890"
						}
					},
				},
				FunctionalError = CreateFunctionalErrorTypeObjects(),
			};
		}

		public static string GetTS316Text(string lrn, DateTime rejectionDate) => IEXmlObjectSerializer.Serialize(CreateTS316Object(lrn, rejectionDate));

		public static Ts328 CreateTS328Object(string mrn, string lrn, DateTime acceptanceDate)
		{
			return new Ts328()
			{
				Declaration = new CargoWise.Customs.IE.MessageDefinitions.AISVersion1_0.TS328.DeclarationType()
				{
					Mrn = mrn,
					Lrn25 = lrn,
					AcceptanceDate = acceptanceDate.ToString(DateFormat),
					ResponseDateLimit = acceptanceDate.AddDays(30).ToString(DateFormat),
					CustomsOffices = new CargoWise.Customs.IE.MessageDefinitions.AISVersion1_0.TS328.DeclarationTypeCustomsOffices()
					{
						CustomsOfficeLodgement = "IE123456"
					},
					Parties = new CargoWise.Customs.IE.MessageDefinitions.AISVersion1_0.TS328.DeclarationTypeParties()
					{
						Declarant = new CargoWise.Customs.IE.MessageDefinitions.AISVersion1_0.TS328.DeclarantType()
						{
							Declarant318 = "IE123890"
						}
					},
					Remarks = "remarks"
				}
			};
		}

		public static string GetTS328Text(string mrn, string lrn, DateTime acceptanceDate) => IEXmlObjectSerializer.Serialize(CreateTS328Object(mrn, lrn, acceptanceDate));

		public static Ts333 CreateTS333Object(string mrn, DateTime rejectionDate, string rejectionReason)
		{
			return new Ts333()
			{
				Declaration = new CargoWise.Customs.IE.MessageDefinitions.AISVersion1_0.TS333.DeclarationType()
				{
					Mrn = mrn,
					RejectionDate = rejectionDate.ToString(DateTimeFormat),
					RejectionReason = rejectionReason,
					CustomsOffices = new CargoWise.Customs.IE.MessageDefinitions.AISVersion1_0.TS333.DeclarationTypeCustomsOffices()
					{
						CustomsOfficeLodgement = "IE123456"
					},
					Parties = new CargoWise.Customs.IE.MessageDefinitions.AISVersion1_0.TS333.DeclarationTypeParties()
					{
						Declarant = new CargoWise.Customs.IE.MessageDefinitions.AISVersion1_0.TS333.DeclarantType()
						{
							Declarant318 = "IE123890"
						}
					},
				},
				FunctionalError = CreateFunctionalErrorTypeObjects(),
			};
		}

		public static string GetTS333Text(string mrn, DateTime rejectionDate, string rejectionReason) => IEXmlObjectSerializer.Serialize(CreateTS333Object(mrn, rejectionDate, rejectionReason));

		public static Ts351 CreateTS351Object(string mrn, string lrn, DateTime controlDate, string remarks)
		{
			return new Ts351()
			{
				Declaration = new CargoWise.Customs.IE.MessageDefinitions.AISVersion1_0.TS351.DeclarationType()
				{
					MsgType = "01",
					Mrn = mrn,
					Lrn25 = lrn,
					ControlResult = new ControlsType
					{
						ControlDate = controlDate.ToString(DateFormat),
						ControlResultCode = "TX",
						Remarks = "control remarks",
					},
					CustomsOffices = new CargoWise.Customs.IE.MessageDefinitions.AISVersion1_0.TS351.DeclarationTypeCustomsOffices()
					{
						CustomsOfficeLodgement = "IE123456"
					},
					Parties = new CargoWise.Customs.IE.MessageDefinitions.AISVersion1_0.TS351.DeclarationTypeParties()
					{
						Declarant = new CargoWise.Customs.IE.MessageDefinitions.AISVersion1_0.TS351.DeclarantType()
						{
							Declarant318 = "IE123890"
						}
					},
					Remarks = remarks
				},
				GoodsShipment = new GoodsShipmentType
				{
					GoodsShipmentItem = new Collection<GoodsShipmentItemType>(new[]
					{
						new GoodsShipmentItemType
						{
							GoodsItemNumber16 = "1",
							GoodsInformation = new GoodsShipmentItemTypeGoodsInformation()
							{
								GoodsDescription68 = "goods desc",
							}
						}
					})
				}
			};
		}

		public static string GetTS351Text(string mrn, string lrn, DateTime controlDate, string remarks) => IEXmlObjectSerializer.Serialize(CreateTS351Object(mrn, lrn, controlDate, remarks));
	}
}
