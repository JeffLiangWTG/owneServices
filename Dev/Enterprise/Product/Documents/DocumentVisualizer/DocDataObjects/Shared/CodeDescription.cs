using System;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using IUniversalCodeDescription = Enterprise.UniversalDataBuss.Integration.ICodeDescriptionDataObject;

namespace Enterprise.DocumentVisualizer.DocDataObjects
{
	[BindTo(nameof(Code))]
	public class CodeDescription : DocDataObject, ICodeDescription
	{
		public CodeDescription(ICodeDescriptionPairList codes)
		{
			this.codes = Argument.NotNull(codes, nameof(codes));
			descriptionProvider = code => codes.GetDescriptionFromCode(code);
		}

		public CodeDescription(IFindBoxListProvider codes)
		{
			this.codes = Argument.NotNull(codes, nameof(codes));
			descriptionProvider = code => codes.DescriptionFromCode(code);
		}

		public CodeDescription(Func<ICodeDescriptionPairList> codeDescriptionPairListFunc)
		{
			this.codeDescriptionPairListFunc = Argument.NotNull(codeDescriptionPairListFunc, nameof(codeDescriptionPairListFunc));
			descriptionProvider = code => codeDescriptionPairListFunc()?.GetDescriptionFromCode(code);
		}

		public static CodeDescription Create(IUniversalCodeDescription universalCodeDescription)
		{
			var codesList = new CodeDescriptionPairList();

			if (universalCodeDescription?.Code.HasValue ?? false)
			{
				codesList.AddPair(universalCodeDescription.Code.GetValueOrDefault(), universalCodeDescription.Description.GetValueOrDefault());
			}

			return new CodeDescription(codesList)
			{
				Code = universalCodeDescription?.Code.GetValueOrDefault() ?? ZString.Empty
			};
		}

		readonly Func<ZString, ZString> descriptionProvider;
		readonly Func<ICodeDescriptionPairList> codeDescriptionPairListFunc;

		#region Code

		[List(nameof(Codes))]
		public ZString Code
		{
			get => code;
			set
			{
				if (SetNonPersistentPropertyValue(CodeInfo, ref code, value))
				{
					Description = descriptionProvider(code);
					Validate(CodeInfo);
				}
			}
		}

		ZString code;

		public ZPropertyInfo CodeInfo => GetZPropertyInfo(nameof(Code));

		#endregion

		#region Description

		public ZString Description
		{
			get => description;
			set
			{
				if (SetNonPersistentPropertyValue(DescriptionInfo, ref description, value))
				{
					Validate(DescriptionInfo);
				}
			}
		}

		ZString description;

		public ZPropertyInfo DescriptionInfo => GetZPropertyInfo(nameof(Description));

		#endregion

		#region Codes

		public object Codes => codes ?? codeDescriptionPairListFunc?.Invoke();
		readonly object codes;

		#endregion
	}
}
