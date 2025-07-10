using System;
using System.Collections.Generic;
using System.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.GUI
{
	public static class RegistryItemEditorFactory
	{
		public delegate RegistryItemEditor NewEditorDelegate(IRegistryItem item, FallbackLevel fallback, IRegistryDataType dataType, IRegistryEditorInfo editorInfo, BusinessObjectFactory factory);
		[ThreadStatic]
		internal static NewEditorDelegate OverridenNewEditorDelegate;

		public static RegistryItemEditor NewEditor(IRegistryItem item, FallbackLevel fallback, IRegistryDataType dataType, IRegistryEditorInfo editorInfo, BusinessObjectFactory factory)
		{
			if (OverridenNewEditorDelegate != null)
			{
				return OverridenNewEditorDelegate(item, fallback, dataType, editorInfo, factory);
			}
			else
			{
				return GetNewEditor(item, fallback, dataType, editorInfo, factory);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1809:AvoidExcessiveLocals")]
		internal static RegistryItemEditor GetNewEditor(IRegistryItem item, FallbackLevel fallback, IRegistryDataType dataType, IRegistryEditorInfo editorInfo, BusinessObjectFactory factory)
		{
			RegistryItemEditor result = null;
			if (editorInfo != null)
			{
				if (editorInfo.GetType() == typeof(TextRegistryEditorInfo))
				{
					var textEditorInfo = editorInfo as TextRegistryEditorInfo;

					if (textEditorInfo.EditorType == TextEditorType.TextBox ||
						textEditorInfo.EditorType == TextEditorType.Memo ||
						textEditorInfo.EditorType == TextEditorType.HTML ||
						textEditorInfo.EditorType == TextEditorType.AWBCustomisableText ||
						textEditorInfo.EditorType == TextEditorType.AWBCustomisableMultilineText ||
						textEditorInfo.EditorType == TextEditorType.Url ||
						textEditorInfo.EditorType == TextEditorType.Guid)
					{
						result = new TextBoxRegistryItemEditor(dataType, textEditorInfo);
					}
					else if (textEditorInfo.EditorType.HasFlag(TextEditorType.Password))
					{
						result = new TextBoxRegistryItemEditor(item, dataType, textEditorInfo);
					}
					else if (textEditorInfo.EditorType == TextEditorType.DirectoryBrowser)
					{
						result = new DirectoryBrowserRegistryItemEditor(dataType);
					}
					else if (textEditorInfo.EditorType == TextEditorType.ScheduleControl)
					{
						result = new ScheduleControlRegistryItemEditor(dataType);
					}
					else if (textEditorInfo.EditorType == TextEditorType.OrgHeaderCodeListEdit)
					{
						result = new OrgHeaderCodeListEditRegistryItemEditor(dataType);
					}
					else if (textEditorInfo.EditorType == TextEditorType.OrgDebtorGroupCodeListEdit)
					{
						result = new OrgDebtorGroupCodeListEditRegistryItemEditor(dataType);
					}
				}
				else if (editorInfo.GetType() == typeof(CodeFindBoxRegistryEditorInfo))
				{
					result = new CodeFindBoxRegistryItemEditor(dataType, editorInfo);
				}
				else if (editorInfo.GetType() == typeof(GuidFindBoxRegistryEditorInfo))
				{
					var info = editorInfo as GuidFindBoxRegistryEditorInfo;
					if (info.FindBoxFilter == RegistryFindBoxFilter.AUCustomsQuarantineChargeCode)
					{
						result = new ASPChargeCodeGuidFindBoxRegistryItemEditor(dataType, editorInfo, fallback, factory);
					}
					else if (info.FindBoxFilter == RegistryFindBoxFilter.DisbursementChargeCode || info.FindBoxFilter == RegistryFindBoxFilter.CustomDeferredChargeCode)
					{
						result = new ChargeCodeGuidFindBoxRegistryItemEditor(dataType, editorInfo, fallback);
					}
					else
					{
						result = new GuidFindBoxRegistryItemEditor(dataType, editorInfo, fallback);
					}
					((GuidFindBoxRegistryItemEditor)result).IsPrimaryKeyFromCodeRequired = info.IsPrimaryKeyFromCodeRequired;
				}
				else if (editorInfo.GetType() == typeof(ComboBoxRegistryEditorInfo))
				{
					var cbrEditorInfo = editorInfo as ComboBoxRegistryEditorInfo;

					if (cbrEditorInfo?.LookUpList?.GetType() == typeof(IncoTermsCodeDescriptionPairList))
					{
						result = new IncoTermComboBoxRegistryItemEditor(dataType, cbrEditorInfo);
					}
					else
					{
						result = new ComboBoxRegistryItemEditor(dataType, cbrEditorInfo);
					}
				}
				else if (editorInfo.GetType() == typeof(ComboBoxFilterLayoutRegistryEditorInfo))
				{
					result = new ComboBoxFilterLayoutRegistryItemEditor(editorInfo, fallback, factory);
				}
				else if (editorInfo.GetType() == typeof(BooleanRegistryEditorInfo))
				{
					result = new RadioButtonRegistryItemEditor(editorInfo, dataType);
				}
				else if (editorInfo.GetType() == typeof(NumericRegistryEditorInfo))
				{
					result = new CalcEditRegistryItemEditor(dataType, editorInfo);
				}
				else if (editorInfo.GetType() == typeof(OrgRequiredFieldsRegistryEditorInfo))
				{
					result = new OrgRequiredFieldsRegistryItemEditor(
						item.Name == RawDataRegistry.Instance.OrgDebtorRequiredFields.Name && item.Category == RawDataRegistry.Categories.Organizations_OrganizationRequiredFields ||
						item.Name == RawDataRegistry.Instance.TempOrgDebtorRequiredFields.Name && item.Category == RawDataRegistry.Categories.Organizations_TempOrganizationRequiredFields, dataType);
				}
				else if (editorInfo.GetType() == typeof(AutoRatingRequiredFieldsRegistryEditorInfo))
				{
					result = new AutoRatingRequiredFieldsRegistryItemEditor((AutoRatingRequiredFieldsRegistryEditorInfo)editorInfo, dataType);
				}
				else if (editorInfo.GetType() == typeof(ZAddressRegistryEditorInfo))
				{
					result = new ZAddressRegistryItemEditor(dataType);
				}
				else if (editorInfo.GetType() == typeof(AutoRatingPriorityRegistryEditorInfo))
				{
					result = new AutoRatingPriorityRegistryItemEditor(dataType);
				}
				else if (editorInfo.GetType() == typeof(OrderedListRegistryItemEditorInfo))
				{
					result = new OrderedListRegistryItemEditor(dataType);
				}
				else if (editorInfo.GetType() == typeof(AuthenticationRegistryItemEditorInfo))
				{
					result = new AuthenticationRegistryItemEditor((editorInfo as AuthenticationRegistryItemEditorInfo).AutoGeneratedPassword, dataType);
				}
				else if (editorInfo.GetType() == typeof(RateValidityRegistryEditorInfo))
				{
					result = new RateValidityRegistryItemEditor(dataType);
				}
				else if (editorInfo.GetType() == typeof(QuoteValidityRegistryEditorInfo))
				{
					result = new QuoteValidityRegistryItemEditor(dataType);
				}
				else if (editorInfo.GetType() == typeof(DepartmentMappingEditorInfo))
				{
					result = new DepartmentMappingRegistryItemEditor(dataType);
				}
				else if (editorInfo.GetType() == typeof(ContactRegistryEditorInfo))
				{
					result = new ContactEditRegistryItemEditor(dataType);
				}
				else if (editorInfo.GetType() == typeof(FileUpLoaderX509CertificateRegistryEditorInfo))
				{
					result = new DigitalCertificateRegistryItemEditor(dataType, editorInfo, fallback);
				}
				else if (editorInfo.GetType() == typeof(HAWBDocumentPivotRegistryEditorInfo))
				{
					result = new HAWBDocumentPivotRegistryItemEditor(dataType);
				}
				else if (editorInfo.GetType() == typeof(MAWBDocumentPivotRegistryEditorInfo))
				{
					result = new MAWBDocumentPivotRegistryItemEditor(dataType);
				}
				else if (editorInfo.GetType() == typeof(MarkUpPercentagesRegistryEditorInfo))
				{
					result = new MarkUpPercentagesRegistryItemEditor(dataType);
				}
				else if (editorInfo.GetType() == typeof(CodeDescriptionPairListEditorInfo))
				{
					result = new CodeDescriptionListRegistryItemEditor(item, dataType, editorInfo);
				}
				else if (editorInfo.GetType() == typeof(LoginPasswordPairListEditorInfo))
				{
					result = new LoginPasswordListRegistryItemEditor(item, dataType, editorInfo);
				}
				else if (editorInfo.GetType() == typeof(FreightDataRegistry.AWBGridRegistryEditorInfo))
				{
					result = new AWBExtraTextControlRegistryItemEditor(item, dataType, editorInfo);
				}
				else if (editorInfo.GetType() == typeof(DateTimeRegistryEditorInfo))
				{
					result = new DateEditRegistryItemEditor(dataType, (DateTimeRegistryEditorInfo)editorInfo);
				}
				else if (editorInfo.GetType() == typeof(CodeDescriptionBoolRegistryEditorInfo))
				{
					result = new CodeDescriptionBoolRegistryItemEditor(dataType, editorInfo, fallback);
				}
				else if (editorInfo.GetType() == typeof(ParentAndChildCodeDescriptionBoolRegistryEditorInfo))
				{
					result = new ParentAndChildCodeDescriptionBoolRegistryItemEditor(dataType, editorInfo);
				}
				else if (editorInfo.GetType() == typeof(AccChargeCodeListRegistryEditorInfo))
				{
					result = new AccChargeCodeListEditRegistryItemEditor(editorInfo, fallback, factory);
				}
				else if (editorInfo.GetType() == typeof(AccTaxRateListRegistryEditorInfo))
				{
					result = new AccTaxRateListEditRegistryItemEditor(editorInfo, fallback, factory);
				}
				else if (editorInfo.GetType() == typeof(CountryListRegistryEditorInfo))
				{
					result = new CountryListRegistryItemEditor(factory);
				}
				else if (editorInfo.GetType() == typeof(WebCustomImagesEditorInfo))
				{
					result = new WebCustomImagesRegistryItemEditor(dataType, item);
				}
				else if (editorInfo.GetType() == typeof(WebCustomCssEditorInfo))
				{
					result = new WebCustomCssRegistryItemEditor(dataType, (WebCustomCssRegistryItem)item);
				}
				else if (editorInfo.GetType() == typeof(WebThemeEditorInfo))
				{
					result = new WebThemeRegistryItemEditor(dataType, item);
				}
				else if (editorInfo.GetType() == typeof(WebEDocsDownloadEditorInfo))
				{
					result = new WebEDocsDownloadRegistryItemEditor(dataType);
				}
				else if (editorInfo.GetType() == typeof(ServiceUrlRegistryEditorInfo))
				{
					result = new ServiceUrlRegistryItemEditor(dataType);
				}
				else if (editorInfo.GetType() == typeof(RatesServiceUrlRegistryEditorInfo))
				{
					result = new RatesServiceUrlRegistryItemEditor(dataType);
				}
				else if (editorInfo.GetType() == typeof(ParameterizedStringRegistryItemEditorInfo))
				{
					result = new ParameterizedStringRegistryItemEditor(item);
				}
				else if (editorInfo.GetType() == typeof(CodeDescriptionWithGroupRegistryEditorInfo))
				{
					result = new CodeDescriptionWithGroupRegistryItemEditor(dataType, editorInfo, fallback);
				}
				else if (editorInfo.GetType() == typeof(CodeDescriptionWithThreeGroupsRegistryEditorInfo))
				{
					result = new CodeDescriptionWithThreeGroupsRegistryItemEditor(dataType, editorInfo, fallback);
				}
				else if (editorInfo is EmailDestinationOverrideEditorInfo textInfo)
				{
					result = new EmailDestinationOverrideItemEditor(dataType, factory, textInfo);
				}
				else if (editorInfo is Ms365OAuth2TokenRegistryEditorInfo)
				{
					result = new ConsentGrantingAndOAuth2TokenUserRegistryItemEditor(item);
				}
				else if (editorInfo is GmailOAuth2JsonFileRegistryEditorInfo)
				{
					result = new GmailOAuth2JsonFileUserRegistryItemEditor(item);
				}
				else if (editorInfo is WebPrintNudgeEditorInfo)
				{
					result = new WebPrintNudgeRegistryItemEditor(dataType, item);
				}
				else if (editorInfo is DeleteExpiredRatesEditorInfo)
				{
					result = new DeleteExpiredRatesRegistryItemEditor(dataType, item);
				}
				else if (editorInfo is WebPrintNudgeSuspendingEditorInfo)
				{
					result = new WebPrintNudgeSuspendingRegistryItemEditor(dataType, item);
				}
				else if (editorInfo is GlowUseIndexingForModuleListEditorInfo)
				{
					result = new GlowUseIndexingForModuleListRegistryItemEditor(dataType);
				}
				else
				{
					var attribute = TypeDescriptor.GetAttributes(editorInfo)[typeof(RegistryEditorAttribute)] as RegistryEditorAttribute;
					if (attribute != null)
					{
						var editorType = Type.GetType(attribute.TypeName);

						var constructor = editorType.GetConstructors()[0];
						var parameters = constructor.GetParameters();

						var args = new List<Object>();

						foreach (var parameter in parameters)
						{
							if (parameter.ParameterType.Equals(typeof(IRegistryDataType)))
							{
								args.Add(dataType);
							}
							else if (parameter.ParameterType.Equals(typeof(FallbackLevel)))
							{
								args.Add(fallback);
							}
							else if (parameter.ParameterType.Equals(typeof(BusinessObjectFactory)))
							{
								args.Add(factory);
							}
							else if (parameter.ParameterType.Equals(typeof(IRegistryItem)))
							{
								args.Add(item);
							}
							else if (parameter.ParameterType.Equals(typeof(IRegistryEditorInfo)))
							{
								args.Add(editorInfo);
							}
						}

						result = (RegistryItemEditor)Activator.CreateInstance(editorType, args.ToArray());
					}

					//IRegistryDataType dataType, FallbackLevel fallbackLevel, BusinessObjectFactory factory
				}

				//else if (EditorInfo.GetType() == typeof(HBLAndHAWBBrandingOptionEditorInfo))
				//{
				//    Result = new HBLAndHAWBBrandingOptionRegistryItemEditor(DataType);
				//}
			}
			else if (dataType.GetType() == typeof(ImageRegistryDataType))
			{
				result = new ImageRegistryItemEditor(dataType);
			}
			else if (dataType.GetType() == typeof(DecimalArrayRegistryDataType))
			{
				result = new DecimalArrayRegistryItemEditor(dataType);
			}
			else if (typeof(StringArrayRegistryDataType).IsAssignableFrom(dataType.GetType()))
			{
				result = new StringArrayRegistryItemEditor(dataType);
			}
			else if (dataType.GetType() == typeof(BinaryKeyRegistryDataType))
			{
				result = new BinaryKeyRegistryItemEditor(dataType);
			}
			else
			{
				RegistryEditorAttribute attribute = TypeDescriptor.GetAttributes(dataType)[typeof(RegistryEditorAttribute)] as RegistryEditorAttribute;
				if (attribute != null)
				{
					Type editorType = Type.GetType(attribute.TypeName);
					result = (RegistryItemEditor)Activator.CreateInstance(editorType, new object[] { dataType, fallback, factory });
				}
			}

			return result;
		}
	}
}
