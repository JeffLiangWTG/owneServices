using CargoWise.Types;
using Enterprise.ClientSharedComponents;
using Enterprise.DataTransfer.Business;

namespace Enterprise.Client.OIA.Business
{
	internal class OIAGLDataRow : SharedFlatFileDataRow
	{
		public OIAGLDataRow() : base(OIAGLDataRow.Schema.FieldCapacity) { }

		public static class Schema
		{
			public static readonly FlatFileFieldProperty LocalClientOrgCode = new FlatFileFieldProperty(0, 15);
			public static readonly FlatFileFieldProperty ARAccountGroup = new FlatFileFieldProperty(1, 15);
			public static readonly FlatFileFieldProperty CustomisableText1 = new FlatFileFieldProperty(2, 255);
			public static readonly FlatFileFieldProperty CustomisableText2 = new FlatFileFieldProperty(3, 255);

			internal const int FieldCapacity = 4;
		}

		public OIAGLHeadingDataRow Heading
		{
			get { return heading ?? (heading = new OIAGLHeadingDataRow()); }
		}
		OIAGLHeadingDataRow heading;

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
			set { SetField(Schema.LocalClientOrgCode, value); }
		}

		public ZString ARAccountGroup
		{
			get { return GetField(Schema.ARAccountGroup); }
			set { SetField(Schema.ARAccountGroup, value); }
		}

		public ZString CustomisableText1
		{
			get { return GetField(Schema.CustomisableText1); }
			set { SetField(Schema.CustomisableText1, value); }
		}

		public ZString CustomisableText2
		{
			get { return GetField(Schema.CustomisableText2); }
			set { SetField(Schema.CustomisableText2, value); }
		}

		public override string DataDateFormat
		{
			get { return dataDateFormat; }
		}
		const string dataDateFormat = "dd/mm/yyyy hh:mm:ss tt";
	}
}
