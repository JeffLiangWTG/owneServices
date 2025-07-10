using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.EU;

namespace Enterprise.Customs.IE.Business.Declaration
{
	public partial class ExportJobDeclarationValidation : CommonExportJobDeclarationValidation
	{
		public ExportJobDeclarationValidation(JobDeclaration parent) : base(parent)
		{
		}

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateOneMainPackInvoiceLinePerPackage();
		}

		void ValidateOneMainPackInvoiceLinePerPackage()
		{
			var declaration = Parent;
			declaration.OneMainPackInvoiceLinePerPackageValidationResult = new Dictionary<ZGuid, ZString>();
			var packagePivots = declaration.InvoiceLines.Cast<JobComInvoiceLine>().Where(invoiceLine => invoiceLine.OverallPackageType == PackageType.Packed && invoiceLine.EntryInstruction != null).SelectMany(x => x.PackagesPivot).Cast<InvoiceLinePackagePivot>();

			var packagePivotsGroups = packagePivots.GroupBy(x => new { x.CHC_CW, x.InvoiceLine.JI_CEI });

			foreach (var packagePivotsGroup in packagePivotsGroups)
			{
				var mainPackInvoices = packagePivotsGroup.Where(pivot => (pivot.InvoiceLine as JobComInvoiceLine).ZG_IsMainPack);
				var mainPackNumber = mainPackInvoices.Count();
				if (mainPackNumber == 0)
				{
					var invoiceAndLineReferences = packagePivotsGroup.Select(x => x.InvoiceLine.InvoiceAndLineReference).ToArray();
					foreach (var pivot in packagePivotsGroup)
					{
						var invoiceLine = pivot.InvoiceLine;
						declaration.OneMainPackInvoiceLinePerPackageValidationResult[invoiceLine.PK] = ExportAddInfoJobComInvoiceLineValidation.GetOneMainPackInvoiceLinePerPackageMesssageNone(invoiceAndLineReferences);
					}
				}
				else if (mainPackNumber > 1)
				{
					var invoiceAndLineReferences = mainPackInvoices.Select(x => x.InvoiceLine.InvoiceAndLineReference).ToArray();
					foreach (var pivot in mainPackInvoices)
					{
						var invoiceLine = pivot.InvoiceLine;
						declaration.OneMainPackInvoiceLinePerPackageValidationResult[invoiceLine.PK] = ExportAddInfoJobComInvoiceLineValidation.GetOneMainPackInvoiceLinePerPackageMesssageMulti(invoiceAndLineReferences);
					}
				}
			}
		}

		protected override void CheckJE_ContainerMode_Mandatory()
		{
			MandatoryValidation.MessageErrorIfNotEntered(Parent.JE_ContainerModeInfo);
		}

		protected override void CheckJE_RL_NKOrigin()
		{
			base.CheckJE_RL_NKOrigin();

			var parent = Parent;
			if (parent.JE_RL_NKOrigin.IsEmpty && !parent.CustomsEntryInstructions.Any(x => x.CEI_Style.EqualsIgnoringCase(ExportDeclarationTypeList.Codes.B4)))
			{
				parent.JE_RL_NKOriginInfo.AddMessageError(Res.GetString("52A3BABB-10C8-4AF0-9BE9-894BAE041303", "Country of Export is mandatory unless Declaration Type is B4."));
			}
		}

		protected override void CheckJE_RL_NKFinalDestination()
		{
			base.CheckJE_RL_NKFinalDestination();

			MandatoryValidation.MessageErrorIfNotEntered(Parent.JE_RL_NKFinalDestinationInfo);
		}

		protected override void CheckJE_GoodsOrigin()
		{
			base.CheckJE_GoodsOrigin();

			var parent = Parent;
			if (parent.JE_GoodsOrigin.IsEmpty && parent.CustomsEntryInstructions.Any(x =>
			{
				var declarationType = x.CEI_Style.ToUpperInvariant();
				return declarationType == ExportDeclarationTypeList.Codes.B2 || declarationType == ExportDeclarationTypeList.Codes.B3;
			}))
			{
				parent.JE_GoodsOriginInfo.AddMessageError(Res.GetString("B124BBD8-1DED-4E16-934E-046E989F10CA", "Country of Origin is required when Declaration Type is B2 or B3."));
			}
		}

		protected override void CheckJE_LocationOtherInformation()
		{
			var parent = Parent;
			if (parent.JE_LocationOtherInformation.IsEmpty)
			{
				if (!Parent.IsAllPreliminaryDeclaration)
				{
					parent.JE_LocationOtherInformationInfo.AddMessageError(Res.GetString("8CCDDB7E-C9A1-4836-A6C4-08E96D42E50B", "Type is mandatory unless all Instruction Sub Style in 'D', 'F'."));
				}
			}
			else
			{
				ListValidation.MessageErrorIfInvalidCode(Parent.JE_LocationOtherInformationInfo);
			}
		}

		protected override void CheckJE_LocationQualifier()
		{
			var parent = Parent;
			if (parent.JE_LocationQualifier.IsEmpty)
			{
				if (!Parent.IsAllPreliminaryDeclaration)
				{
					parent.JE_LocationQualifierInfo.AddMessageError(Res.GetString("8CCDDB7E-C9A1-4836-A6C4-08E96D42E50C", "Qualifier is mandatory unless all Instruction Sub Style in 'D', 'F'."));
				}
			}
			else
			{
				ListValidation.MessageErrorIfInvalidCode(Parent.JE_LocationQualifierInfo);
			}
		}

		protected override void CheckJE_LocationOfGoods()
		{
			var parent = Parent;
			if (parent.JE_LocationOfGoods.IsEmpty)
			{
				if (!Parent.IsAllPreliminaryDeclaration)
				{
					parent.JE_LocationOfGoodsInfo.AddMessageError(Res.GetString("8CCDDB7E-C9A1-4836-A6C4-08E96D42E50D", "Goods Location is mandatory unless all Instruction Sub Style in 'D', 'F'."));
				}
			}
			else
			{
				ListValidation.MessageErrorIfInvalidCode(Parent.JE_LocationOfGoodsInfo);
			}
		}

		protected override void CheckJE_EntryStyle()
		{
			base.CheckJE_EntryStyle();
			CheckNoAmendingOnEntryStatus(Parent.OriginalDeclarationType, Parent.JE_EntryStyleInfo);
		}

		protected override void CheckJE_CustomsOffice()
		{
			base.CheckJE_CustomsOffice();
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.JE_CustomsOfficeInfo);
			CheckNoAmendingOnEntryStatus(Parent.OriginalCustomsOffice, Parent.JE_CustomsOfficeInfo);
		}

		protected override void CheckJE_TransportMeans()
		{
			var parent = Parent;
			var info = parent.JE_TransportMeansInfo;
			var officeOfExport = parent.JE_CustomsOffice;

			var isDirectExport = !officeOfExport.IsEmpty && (officeOfExport == parent.OfficeOfExitCustomsOffice);

			if (isDirectExport)
			{
				CheckJE_TransportMeans_DirectExport(info);
			}
			else
			{
				CheckJE_TransportMeans_IndirectExport(info);
			}
		}

		protected void CheckJE_TransportMeans_DirectExport(ZPropertyInfo transportMeansInfo)
		{
			MandatoryValidation.MessageErrorIfIsEntered(transportMeansInfo);
		}

		protected void CheckJE_TransportMeans_IndirectExport(ZPropertyInfo info)
		{
			var parent = Parent;
			switch (parent.JE_TransportModeInland)
			{
				case TransportTypeList.Codes.Air:
				case TransportTypeList.Codes.InlandWaterwayTransport:
				case TransportTypeList.Codes.OwnPropulsion:
				case TransportTypeList.Codes.Rail:
				case TransportTypeList.Codes.Sea:
					TransportMeansDefaultValidation(info, parent);
					break;

				case TransportTypeList.Codes.Road:
					TransportMeansRoadValidation(info, parent);
					break;

				case TransportTypeList.Codes.FixedTransportInstallations:
				case TransportTypeList.Codes.Mail:
					TransportMeansFixAndMailValidation(info, parent);
					break;
			}
		}

		static void TransportMeansDefaultValidation(ZPropertyInfo info, JobDeclaration parent)
		{
			var notRequired = false;
			foreach (var style in parent.CustomsEntryInstructions.Select(x => x.CEI_Style.ToUpperInvariant()))
			{
				switch (style)
				{
					case ExportDeclarationTypeList.Codes.B1:
					case ExportDeclarationTypeList.Codes.B2:
					case ExportDeclarationTypeList.Codes.B3:
						ListValidation.MessageErrorIfInvalidCodeOrEmpty(info);
						return;
					case ExportDeclarationTypeList.Codes.B4:
					case ExportDeclarationTypeList.Codes.C1:
						notRequired = true;
						break;
				}
			}
			if (notRequired)
			{
				MandatoryValidation.MessageErrorIfIsEntered(info);
			}
		}

		static void TransportMeansRoadValidation(ZPropertyInfo info, JobDeclaration parent)
		{
			var isSingleContract = parent.InvoiceLines.Cast<JobComInvoiceLine>()
				.Any(x => x.AdditionalInfos.Cast<AdditionalInfo>()
					.Any(a => a.IsAnAdditionalInformation
						&& a.CSI_Code.EqualsIgnoringCase(Constants.ExportAdditionalInformationCodes.SingleContract)));
			if (!isSingleContract)
			{
				TransportMeansDefaultValidation(info, parent);
			}
			else if (!info.Value.IsEmpty)
			{
				info.AddMessageError(Res.GetString("10ED4D5F-6A5A-4FCA-B2F9-301D951CF3B9", "When a shipment has a single contract, then {0} is not Allowed", info.HumanReadableName));
			}
		}

		static void TransportMeansFixAndMailValidation(ZPropertyInfo info, JobDeclaration parent)
		{
			var notRequired = false;
			foreach (var style in parent.CustomsEntryInstructions.Select(x => x.CEI_Style.ToUpperInvariant()))
			{
				switch (style)
				{
					case ExportDeclarationTypeList.Codes.B1:
						ListValidation.MessageErrorIfInvalidCode(info);
						return;
					case ExportDeclarationTypeList.Codes.B2:
					case ExportDeclarationTypeList.Codes.B3:
					case ExportDeclarationTypeList.Codes.B4:
					case ExportDeclarationTypeList.Codes.C1:
						notRequired = true;
						break;
				}
			}
			if (notRequired)
			{
				MandatoryValidation.MessageErrorIfIsEntered(info);
			}
		}

		protected override void CheckJE_TransportModeMandatory()
		{
			var parent = Parent;
			if (parent.JE_TransportMode.IsEmpty)
			{
				var instructionB1 = parent.CustomsEntryInstructions.FirstOrDefault(x => x.CEI_Style.EqualsIgnoringCase(ExportDeclarationTypeList.Codes.B1));
				if (instructionB1 == null)
				{
					var info = parent.JE_TransportModeInfo;
					info.AddMessageError(MandatoryValidation.YouHaveNotEnteredMessage(info.HumanReadableName));
				}
				else
				{
					switch (instructionB1.CEI_SubStyle.ToUpperInvariant())
					{
						case EU.Business.EntrySubStyleList.Codes.IncompleteDeclaration:
						case EU.Business.EntrySubStyleList.Codes.SimplifiedDeclaration:
						case EU.Business.EntrySubStyleList.Codes.PreliminaryDeclarationUnderCodeB:
						case EU.Business.EntrySubStyleList.Codes.PreliminaryDeclarationUnderCodeC:
							break;
						default:
							MandatoryValidation.AddYouHaveNotEnteredMessage(parent.JE_TransportModeInfo);
							break;
					}
				}
			}
		}

		protected override void CheckJE_TransportModeInland()
		{
			base.CheckJE_TransportModeInland();
			var parent = Parent;
			var transportModeInlandInfo = Parent.JE_TransportModeInlandInfo;

			if (parent.CustomsEntryInstructions.Select(x => x.CEI_Style.ToUpperInvariant()).Any(x => x == ExportDeclarationTypeList.Codes.B1 || x == ExportDeclarationTypeList.Codes.B2 || x == ExportDeclarationTypeList.Codes.B3))
			{
				var officeOfExport = parent.JE_CustomsOffice;

				if (!parent.JE_TransportModeInland.IsEmpty)
				{
					if (!officeOfExport.IsEmpty && (officeOfExport == parent.OfficeOfExitCustomsOffice || officeOfExport == parent.PresentationCustomsOffice))
					{
						transportModeInlandInfo.AddMessageError(Res.GetString("F416A176-B7C3-460C-B6EC-1BCAEB34784C", "When Office of Export equals Office of Exit or Office of Presentation, then {0} is not Allowed", transportModeInlandInfo.HumanReadableName));
					}
				}
				else if (!officeOfExport.IsEmpty)
				{
					if ((parent.OfficeOfExitCustomsOffice is ZString officeOfExit && !officeOfExit.IsEmpty && officeOfExport != officeOfExit)
						|| (parent.PresentationCustomsOffice is ZString presentationOffice && !presentationOffice.IsEmpty && officeOfExport != presentationOffice))
					{
						var entriesExistWithSubStyleRequiringInlandTransportMode = parent.CustomsEntryInstructions.Any(x =>
						{
							switch (x.CEI_SubStyle.ToUpperInvariant())
							{
								case EU.Business.EntrySubStyleList.Codes.IncompleteDeclaration:
								case EU.Business.EntrySubStyleList.Codes.SimplifiedDeclaration:
								case EU.Business.EntrySubStyleList.Codes.PreliminaryDeclarationUnderCodeA:
								case EU.Business.EntrySubStyleList.Codes.PreliminaryDeclarationUnderCodeB:
								case EU.Business.EntrySubStyleList.Codes.PreliminaryDeclarationUnderCodeC:
									return false;
								default:
									return true;
							}
						});

						if (entriesExistWithSubStyleRequiringInlandTransportMode || parent.CustomsEntryHeaders.Any(entry => entry.CH_EntryStatus == AESEntryStatusList.Codes.Prelodged))
						{
							transportModeInlandInfo.AddMessageError(Res.GetString("391673C5-AD0E-43E4-9F03-7F6F4657C6A8", "{0} is mandatory for indirect exits.", transportModeInlandInfo.HumanReadableName));
						}
					}
				}
			}
		}
	}
}
