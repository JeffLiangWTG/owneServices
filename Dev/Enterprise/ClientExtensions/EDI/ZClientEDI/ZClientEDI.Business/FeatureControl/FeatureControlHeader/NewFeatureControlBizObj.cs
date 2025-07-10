using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.FeatureControl.Business
{
	public class NewFeatureControlBizObj : NonPersistentBusinessObject, IObsoleteValidation
	{
		public NewFeatureControlBizObj()
		{
			FeatureControlCodeList = GetCodeList();
			if (FeatureControlCodeList.Count > 0)
			{
				foreach (var code in new BusinessObjectFactory().Load<FeatureControlHeader>(new ZQuery(FeatureControlHeaderSchema.FCM_FeatureControlCode, FeatureControlCodeList.GetAllCodes())).Select(x => x.FCM_FeatureControlCode))
				{
					FeatureControlCodeList.RemoveCode(code);
				}
			}
		}

		[ResourceStringData("NewFeatureControlBizObj|FeatureControlCode", Caption = "Code")]
		[List("FeatureControlCodeList")]
		[MaxLength(FeatureControlHeader.Schema.FCM_FeatureControlCodeMaxLength)]
		public ZString FeatureControlCode
		{
			get => featureControlCode;
			set
			{
				SetNonPersistentPropertyValue(FeatureControlCodeInfo, ref featureControlCode, value);
				RunPreSaveValidationCore();
			}
		}

		ZString featureControlCode;

		public ZPropertyInfo FeatureControlCodeInfo => GetZPropertyInfo(nameof(FeatureControlCode));

		public CodeDescriptionPairList FeatureControlCodeList { get; }

		protected override void RunPreSaveValidationCore()
		{
			FeatureControlCodeInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(FeatureControlCodeInfo);
			ListValidation.ErrorIfInvalidCode(FeatureControlCodeInfo);
			base.RunPreSaveValidationCore();
		}

		protected virtual CodeDescriptionPairList GetCodeList()
		{
			var list = new CodeDescriptionPairList();
			foreach (var pair in CargoWise.Definitions.LicenceFeatureCodeList.GetLicenceFeatureCodePairs())
			{
				list.AddPair(pair.Code, pair.Description);
			}
			list.AddPairsIfNotExist(EDIDataRegistry.Instance.FeatureControlCodeList.Value.OfType<ICodeDescription>());
			return list;
		}
	}
}
