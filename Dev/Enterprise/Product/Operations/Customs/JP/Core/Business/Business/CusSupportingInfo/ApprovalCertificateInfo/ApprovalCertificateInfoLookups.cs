using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.JP.Common;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.JP.Business
{
	public class ApprovalCertificateInfoLookups : Customs.Business.CusSupportingInfoLookups
	{
		public ApprovalCertificateInfoLookups(ApprovalCertificateInfo parent) : base(parent)
		{
			declaration = parent.EntryInstruction?.JobDeclaration;
			this.parent = parent;
		}

		readonly JobDeclaration declaration;
		readonly ApprovalCertificateInfo parent;

		public CodeDescriptionPairList ApprovalCertificateType
		{
			get
			{
				var codeDescriptionList = new CodeDescriptionPairList();

				if (declaration == null)
				{
					return codeDescriptionList;
				}

				var transportMode = declaration.TransportMode;
				var date = ZDateTime.Today;
				var codeType = declaration.IsExport ? Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ExportApprovalCertificateType : Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ImportApprovalCertificateNumber;

				var codes = Factory.GetCachedValue($"JPApprovalCertificateInfoLookups.ApprovalCertificateType+{transportMode}+{codeType}+{date}", () =>
				{
					return ZZRefCusCodeListCombined.Loader.Load(Factory, Core.Constants.CountryCodes.Japan, codeType, date, transportMode: transportMode);
				});

				codeDescriptionList.AddRange(codes);

				return codeDescriptionList;
			}
		}

		public CodeDescriptionPairList ApprovalCertificateNumberList
		{
			get
			{
				var codeDescriptionList = new CodeDescriptionPairList();

				if (parent.CSI_Code == ApprovalCertificateInfoCodes.GENS)
				{
					return codeDescriptionList;
				}

				var date = ZDateTime.Today;
				var transportMode = declaration.TransportMode;
				var certificateType = parent.CSI_Code;
				var attributeFilter = new RefCusCodeListAttributeFilter(RefCusCodeListAttributeTypes.Codes.CertificateType, SQLComparisonOperator.Equal, certificateType).Filter;
				var codeType = declaration.IsExport ? Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ExportConstantApprovalCertificateType : Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ImportConstantApprovalCertificateNumber;

				var codes = Factory.GetCachedValue($"JPApprovalCertificateInfoLookups.ApprovalCertificateNumberList+{transportMode}+{certificateType}+{codeType}+{date}", () =>
				{
					return ZZRefCusCodeListCombined.Loader.Load(Factory, Core.Constants.CountryCodes.Japan, codeType, date, attributeFilter, transportMode: transportMode);
				});

				if (codes.Length != 0)
				{
					codeDescriptionList.AddRange(codes);
				}
				else
				{
					codeDescriptionList.AddPair("KIJI", Res.GetString("D9BFF8AA-16B6-4662-BF1D-784CBC6BBCF4", "The number is over 20 characters and will be entered in Notes for Customs."));
				}

				if (Parent.CSI_Code == ApprovalCertificateInfoCodes.ITNO)
				{
					for (var i = codeDescriptionList.Count - 1; i >= 0; i--)
					{
						var codeItem = codeDescriptionList[i];
						if (codeItem.Code.EndsWith(Core.Constants.CountryCodes.Japan))
						{
							codeDescriptionList.Remove(codeItem);
						}
					}
				}

				return codeDescriptionList;
			}
		}
	}
}
