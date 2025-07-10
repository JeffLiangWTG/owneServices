using System;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.ZArchitecture.GUI.Internal.Testing
{
	public class DummyWithCodes : DummyBusinessObject
	{
		public DummyWithCodes(BusinessObjectFactory factory, DataRow row) : base(factory, row) { }

		[List("Codes")]
		public ZString Code { get; set; }

		[List("Codes")]
		public ZString CodeWithFormat
		{
			get => $"{codeWithFormat} is formatted";
			set
			{
				codeWithFormat = value;
				CodeWithFormatInfo.RefreshBinding();
			}
		}

		ZString codeWithFormat;

		public ZPropertyInfo CodeWithFormatInfo => GetZPropertyInfo(nameof(CodeWithFormat));

		public ReferenceableCollection Codes
		{
			get
			{
				var result = new ReferenceableCollection(Factory);
				result.GetCodePropertyNameForTesting = GetCodePropertyNameForTesting;
				return result;
			}
		}
		public Func<Type, string> GetCodePropertyNameForTesting;

		[List("Collection")]
		public override ZString Z0_Code
		{
			get { return base.Z0_Code; }
			set { base.Z0_Code = value; }
		}
	}
}
