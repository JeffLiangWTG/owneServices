using System;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.ZArchitecture.Business
{
	public class Parameter : NonPersistentBusinessObject, IObsoleteValidation
	{
		#region Schema

		public static class Schema
		{
			public const string Code = "Code";
			public const int CodeMaxLength = 3;

			public const string ParamValue = "ParamValue";
			public const int FixedLengthToBeExcluded = 2;
		}

		#endregion

#if DEBUG
		public Parameter() //May not always make sense to require an eventCode in tests
		{
			EventCode = Events.CustomisableEvent00Code;
		}
#endif

		public Parameter(String eventCode) { EventCode = eventCode; }

		public Parameter(String eventCode, String code, String paramValue)
		{
			Code = code;
			ParamValue = paramValue;
			EventCode = eventCode;
		}

		#region Properties

		string EventCode { get; set; }

		#region Code

		[List("CodeList")]
		[MaxLength(Schema.CodeMaxLength)]
		public ZString Code
		{
			get { return code; }

			set
			{
				if (code != value)
				{
					SetNonPersistentPropertyValue(CodeInfo, ref code, value);

					if (!IsValidationSuspended)
					{
						ValidateCode();
					}
				}
			}
		}

		ZString code;

		public ZPropertyInfo CodeInfo
		{
			get { return GetZPropertyInfo(Schema.Code); }
		}

		#endregion

		#region Parameter Value

		[MaxLength(StmALog.Schema.SL_ReferenceMaxLength - Schema.FixedLengthToBeExcluded)]
		public ZString ParamValue
		{
			get { return paramValue; }

			set
			{
				if (paramValue != value)
				{
					SetNonPersistentPropertyValue(ParamValueInfo, ref paramValue, value);
				}
			}
		}

		ZString paramValue;

		public ZPropertyInfo ParamValueInfo
		{
			get { return GetZPropertyInfo(Schema.ParamValue); }
		}

		#endregion

		#endregion

		#region Validations

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			ValidateCode();
		}

		public void ValidateCode()
		{
			CodeInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(CodeInfo);

			if (!CodeInfo.HasErrors() && !IsUniqueCodeInTheList)
			{
				CodeInfo.AddError(Res.GetString("3cc2237e-0c55-4119-8b3f-88006d72f562", "The parameter {0} has been duplicated and must be unique.", Code));
			}
		}

		public void ValidateParamValue()
		{
			ParamValueInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(ParamValueInfo);
		}

		#region Is Unique Code In The List

		protected virtual bool IsUniqueCodeInTheList
		{
			get
			{
				var parentCollection = ParentCollections.FirstOrDefault() as ParameterCollection;

				return parentCollection == null
						|| parentCollection.Cast<Parameter>().All(parameter => parameter == this || parameter.Code != Code);
			}
		}

		#endregion

		#endregion

		#region Lookups

		public CodeDescriptionPairList CodeList
		{
			get
			{
				return GetCodeListCore();
			}
		}

		protected virtual CodeDescriptionPairList GetCodeListCore()
		{
			if (parametersList == null)
			{
				parametersList = ParameterLookupsProvider.GetParameterCodes(EventCode);
			}
			return parametersList;
		}

		CodeDescriptionPairList parametersList;

		#endregion
	}
}
