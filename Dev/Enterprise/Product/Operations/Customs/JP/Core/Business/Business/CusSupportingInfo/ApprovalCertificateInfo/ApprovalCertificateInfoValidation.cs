using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using Enterprise.Customs.JP.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Customs.JP.Business.JPImportDeclarationTypeList;

namespace Enterprise.Customs.JP.Business
{
	public class ApprovalCertificateInfoValidation : Customs.Business.CusSupportingInfoValidation
	{
		public ApprovalCertificateInfoValidation(ApprovalCertificateInfo bizObj)
			: base(bizObj)
		{
		}

		new ApprovalCertificateInfo Parent => (ApprovalCertificateInfo)base.Parent;

		protected override void CheckCSI_Code()
		{
			base.CheckCSI_Code();

			var info = Parent.CSI_CodeInfo;
			ValidateIDAndNumber(info);
			ValidateCSI_ReferenceNumber();

			var entryInstruction = Parent.EntryInstruction;
			if (entryInstruction != null)
			{
				var code = Parent.CSI_Code;

				var declaration = entryInstruction.JobDeclaration;
				var invoiceLines = entryInstruction.InvoiceLines.Cast<JobComInvoiceLine>().ToArray();
				if (declaration != null)
				{
					var approvalCertificateInfoCollection = entryInstruction.ApprovalCertificateInfos;
					var declarationType = entryInstruction.CEI_Style;

					if (declaration.IsImport)
					{
						if (code == ApprovalCertificateInfoCodes.GKNO && new TakeoverDeclarationTypeList().ContainsCode(declarationType))
						{
							info.AddMessageError(Res.GetString("74D2A798-5817-48EE-BCA5-C5F2F9ED2DE9", "Approval Certificate Type of type GKNO cannot be used when Declaration Type is H,N."));
						}

						if (code == ApprovalCertificateInfoCodes.KANS)
						{
							switch (declarationType)
							{
								case JPImportDeclarationTypeList.Codes.S:
								case JPImportDeclarationTypeList.Codes.M:
								case JPImportDeclarationTypeList.Codes.A:
								case JPImportDeclarationTypeList.Codes.G:
									info.AddMessageError(Res.GetString("C5119DF2-675E-41AB-86F1-89B3CB53EC22", "Approval Certificate Type of type KANS cannot be used when Message Type is IMP and Declaration Type is S, M, A or G."));
									break;
							}
						}

						if (code == ApprovalCertificateInfoCodes.HKAT)
						{
							switch (declarationType)
							{
								case JPImportDeclarationTypeList.Codes.H:
								case JPImportDeclarationTypeList.Codes.N:
								case JPImportDeclarationTypeList.Codes.J:
								case JPImportDeclarationTypeList.Codes.P:
								case JPImportDeclarationTypeList.Codes.R:
									info.AddMessageError(Res.GetString("8A4E03DF-82A5-4169-9E93-C17A1E873E70", "Approval Certificate Type of type HKAT cannot be used when Message Type is IMP and Declaration Type is H, N, J, P, or R."));
									break;
							}
						}

						if (invoiceLines.Any(x => x.JI_Tariff.Length == 6))
						{
							switch (code)
							{
								case ApprovalCertificateInfoCodes.JKAK:
								case ApprovalCertificateInfoCodes.JKAJ:
								case ApprovalCertificateInfoCodes.ILNJ:
								case ApprovalCertificateInfoCodes.ILNO:
									info.AddMessageError(Res.GetString("E2A0E21B-3EC6-474E-A76C-AFA22E65EBDC", "Approval Certificate Type of type ILNJ, ILNO, JKAJ, JKAK cannot be used when Tariff Code is 6 characters long."));
									break;
							}
						}

						if (code == ApprovalCertificateInfoCodes.GENS)
						{
							var approvalCertificateInfoList = approvalCertificateInfoCollection.ToList<ApprovalCertificateInfo>();
							var indexOfThisApprovalCertificateInfo = approvalCertificateInfoList.IndexOf(Parent);

							var hasAdjacentGENS = false;
							if (indexOfThisApprovalCertificateInfo > 0)
							{
								var previousApprovalCertificateInfo = approvalCertificateInfoList[indexOfThisApprovalCertificateInfo - 1];
								if (previousApprovalCertificateInfo.CSI_Code == ApprovalCertificateInfoCodes.GENS)
								{
									hasAdjacentGENS = true;
								}
							}

							if (!hasAdjacentGENS && indexOfThisApprovalCertificateInfo + 1 < approvalCertificateInfoList.Count)
							{
								var nextApprovalCertificateInfo = approvalCertificateInfoList[indexOfThisApprovalCertificateInfo + 1];
								if (nextApprovalCertificateInfo.CSI_Code == ApprovalCertificateInfoCodes.GENS)
								{
									hasAdjacentGENS = true;
								}
							}

							if (!hasAdjacentGENS)
							{
								info.AddMessageError(Res.GetString("DD480B1A-13A2-47B1-B65C-829A56DC3B5B", "Please make sure to input eC/O key and then C/O number."));
							}
						}
					}
					else
					{
						if (code == ApprovalCertificateInfoCodes.MOTS && invoiceLines.All(l => l.OtherLaws.Cast<CusOtherLawReferenceForInvoiceLines>().All(o => o.CFR_Reference != CusOtherLawReference.Constants.MS)))
						{
							info.AddMessageError(Res.GetString("543365EB-9396-48F0-A410-A31FE3D1615B", "Approval Certificate MOTS should not be entered when Other Laws and Regulations Code MS is not entered."));
						}
					}
				}
			}

			ListValidation.MessageErrorIfInvalidCode(Parent.CSI_CodeInfo);
		}

		protected override void CheckCSI_ReferenceNumber()
		{
			base.CheckCSI_ReferenceNumber();

			var parent = Parent;
			var info = parent.CSI_ReferenceNumberInfo;
			ValidateIDAndNumber(info);
			ValidateCSI_Code();

			var entryInstruction = parent.EntryInstruction;
			var declaration = entryInstruction?.JobDeclaration;
			var referenceNumber = parent.CSI_ReferenceNumber;

			if (declaration != null)
			{
				if (declaration.IsExport)
				{
					if (parent.CSI_Code == ApprovalCertificateInfoCodes.AEOU)
					{
						CustomsRegistrationNumberValidation.ValidateCustomsCode(NotificationType.MessageError, OrgCusCode.JapanCodeTypes.NUC, parent.CSI_ReferenceNumber, info);
					}

					if (parent.CSI_Code == ApprovalCertificateInfoCodes.ITNO)
					{
						if (!new Regex(@"^[A-Z0-9]{13}$").IsMatch(referenceNumber))
						{
							info.AddMessageError(Res.GetString("FA7BB3DF-3EA0-48B0-991E-25970189DBE6", "ITNO must be 13 characters long"));
						}

						var referenceNumberLength = referenceNumber.Length;
						var lastTwoCharacter = referenceNumber.Substring(referenceNumberLength - 2);
						var allCountry = parent.Factory.Load<RefCountry>(new ZQuery(RefCountrySchema.RN_IsActive, true));
						if (referenceNumberLength < 2 || !allCountry.Any(x => x.Code.Equals(lastTwoCharacter)))
						{
							info.AddMessageError(Res.GetString("FEC5A4CF-B265-424E-8E53-7A0CC2D11186", "The last two characters must be the country code of the original export country."));
						}
					}

					if (parent.CSI_Code == ApprovalCertificateInfoCodes.MOTS)
					{
						if (!new Regex(@"^M[0-9]{9}$").IsMatch(referenceNumber))
						{
							info.AddMessageError(Res.GetString("91642707-267B-4ED0-9C58-EE80556CD663", "MOTS must start with M followed by 9 digits."));
						}
					}
				}

				if (parent.CSI_Code == ApprovalCertificateInfoCodes.HFNN || parent.CSI_Code == ApprovalCertificateInfoCodes.HFNO)
				{
					if (!new Regex(@"^[A-Z0-9]{11}$").IsMatch(referenceNumber))
					{
						info.AddMessageError(Res.GetString("0D36A364-3EA7-4437-BBAC-8C781222BE4F", "{0} must be 11 characters long.", parent.CSI_Code));
					}
				}
			}

			var certificateNumbers = parent.Lookups.ApprovalCertificateNumberList;
			if (!(certificateNumbers.Count == 1 && certificateNumbers.ContainsCode(ApprovalCertificateInfoCodes.KIJI) || declaration.IsImport && parent.CSI_Code == ApprovalCertificateInfoCodes.GENS))
			{
				ListValidation.MessageErrorIfInvalidCode(info);
			}
		}

		void ValidateIDAndNumber(ZPropertyInfo info)
		{
			if (Parent.CSI_Code.IsEmpty || Parent.CSI_ReferenceNumber.IsEmpty)
			{
				info.AddMessageError(Res.GetString("3C7BCFC6-8CE3-486A-BF87-07F1B8DB075E", "Please enter a set of IDs and numbers."));
			}
			else
			{
				var entryInstruction = Parent.EntryInstruction;
				if (entryInstruction != null &&
					((IEnumerable<ApprovalCertificateInfo>)entryInstruction.ApprovalCertificateInfos).Any(info => info != Parent && info.CSI_Code == Parent.CSI_Code && info.CSI_ReferenceNumber == Parent.CSI_ReferenceNumber))
				{
					Parent.AddRowWarning(Res.GetString("2B673BD2-AE5B-444F-8797-9B83F4ABE39C", "The same information has been entered."));
				}
			}
		}
	}
}
