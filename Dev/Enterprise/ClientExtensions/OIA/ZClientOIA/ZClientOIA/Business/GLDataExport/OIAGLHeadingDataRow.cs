using CargoWise.Types;
using Enterprise.ClientSharedComponents;
using Enterprise.DataTransfer.Business;

namespace Enterprise.Client.OIA.Business
{
	internal class OIAGLHeadingDataRow : SharedFlatFileDataRow
	{
		public OIAGLHeadingDataRow() : base(OIAGLHeadingDataRow.Schema.FieldCapacity)
		{
			this.LocalClientOrgCode = "LocalClient";
			this.ARAccountGroup = "ARGroup";
			this.CustomisableText1 = "Text1";
			this.CustomisableText2 = "Text2";
		}

		public static class Schema
		{
			public static readonly FlatFileFieldProperty LocalClientOrgCode = new FlatFileFieldProperty(0, 11);
			public static readonly FlatFileFieldProperty ARAccountGroup = new FlatFileFieldProperty(1, 7);
			public static readonly FlatFileFieldProperty CustomisableText1 = new FlatFileFieldProperty(2, 5);
			public static readonly FlatFileFieldProperty CustomisableText2 = new FlatFileFieldProperty(3, 5);

			internal const int FieldCapacity = 4;
		}

		protected override void AddFieldProperties()
		{
			FieldProperties.Add(Schema.LocalClientOrgCode);
			FieldProperties.Add(Schema.ARAccountGroup);
			FieldProperties.Add(Schema.CustomisableText1);
			FieldProperties.Add(Schema.CustomisableText2);
		}

		public ZString LocalClientOrgCode
		{
			get { return GetField(Schema.LocalClientOrgCode); }
			private set { SetField(Schema.LocalClientOrgCode, value); }
		}

		public ZString ARAccountGroup
		{
			get { return GetField(Schema.ARAccountGroup); }
			private set { SetField(Schema.ARAccountGroup, value); }
		}

		public ZString CustomisableText1
		{
			get { return GetField(Schema.CustomisableText1); }
			private set { SetField(Schema.CustomisableText1, value); }
		}

		public ZString CustomisableText2
		{
			get { return GetField(Schema.CustomisableText2); }
			private set { SetField(Schema.CustomisableText2, value); }
		}
	}
}
