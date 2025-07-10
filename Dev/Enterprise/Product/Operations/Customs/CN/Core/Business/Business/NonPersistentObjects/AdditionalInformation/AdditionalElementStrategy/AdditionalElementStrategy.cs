using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;

namespace Enterprise.Customs.CN.Business
{
	public interface IAdditionalElementStrategy
	{
		int MaxLength { get; }
		bool IsMandatory { get; }
		bool ProvideList { get; }
		bool IsMergeKey { get; }
		bool IsApplicable(EnteringOrExiting isEnteringOrExiting);
		ICodeDescriptionPairList GetList(BusinessObjectFactory factory, EnteringOrExiting enteringOrExiting);
		ZString GetDefaultValue(EnteringOrExiting isEnteringOrExiting);
		ZString GetFormatCheckMessage(ZString value);
		void ValidateAdditionalElement(IAdditionalInformationWrapperParent master, ZString elementValue, ZPropertyInfo propertyInfo);
	}

	public class CommonAdditionalElementStrategy : IAdditionalElementStrategy
	{
		public virtual int MaxLength => AdditionalInformation.Schema.CY_DataMaxLength;

		public virtual bool IsMandatory => true;

		public virtual bool ProvideList => false;

		public virtual bool IsMergeKey => false;

		public virtual bool IsApplicable(EnteringOrExiting isEnteringOrExiting) => true;

		public virtual ICodeDescriptionPairList GetList(BusinessObjectFactory factory, EnteringOrExiting enteringOrExiting) => null;

		public virtual ZString GetDefaultValue(EnteringOrExiting isEnteringOrExiting) => ZString.Empty;

		public virtual ZString GetFormatCheckMessage(ZString value)
		{
			var result = ZString.Empty;
			if (!value.IsEmpty && !ValidationPattern.IsEmpty && !Regex.IsMatch(value, ValidationPattern))
			{
				result = ValidationPatternMessage;
				if (result.IsEmpty)
				{
					result = Res.GetString("2d3092e9-a8c7-471f-8219-c7c17ed60c94", "The value is in incorrect format.");
				}
			}
			return result;
		}

		protected virtual ZString ValidationPattern => ZString.Empty;

		protected virtual ZString ValidationPatternMessage => ZString.Empty;

		public virtual void ValidateAdditionalElement(IAdditionalInformationWrapperParent master, ZString elementValue, ZPropertyInfo propertyInfo) { }
	}

	public enum EnteringOrExiting
	{
		Both = 0,
		Entering = 1,
		Exiting = 2
	}
}
