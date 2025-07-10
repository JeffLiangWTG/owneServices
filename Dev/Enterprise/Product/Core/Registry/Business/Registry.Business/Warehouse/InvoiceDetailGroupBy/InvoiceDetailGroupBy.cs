using System.Xml;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business.Warehouse
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class InvoiceDetailGroupBy : RegistryBusinessObjectTemplate
	{
		#region Schema

		public abstract class Schema
		{
			public const string Group1 = "Group1";
			public const string Group2 = "Group2";
			public const string Group3 = "Group3";
		}

		#endregion

		#region Business Object overrides

		protected override void SetCustomDefaultValuesCore()
		{
			base.SetCustomDefaultValuesCore();
			Group1 = "JTY";
		}

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();

			ValidateGroup1();
			ValidateGroup2();
			ValidateGroup3();
		}

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new InvoiceDetailGroupBy();
		}

		#endregion

		#region Properties

		#region Group 1

		[CargoWise.ComponentModel.MaxLength(3)]
		public ZString Group1
		{
			get { return group1; }
			set
			{
				CheckMaximumLength(Group1Info, value);
				SetNonPersistentPropertyValue<ZString>(Group1Info, ref group1, value);
				if (!IsValidationSuspended)
				{
					ValidateGroup1();
				}
			}
		}

		public ZPropertyInfo Group1Info
		{
			get { return GetZPropertyInfo(Schema.Group1); }
		}

		public void ValidateGroup1()
		{
			Group1Info.ClearAllNotifications();
			ListValidation.ErrorIfInvalidCode(Group1Info, GroupByList);
		}

		ZString group1;

		#endregion

		#region Group 2

		[CargoWise.ComponentModel.MaxLength(3)]
		public ZString Group2
		{
			get { return group2; }
			set
			{
				CheckMaximumLength(Group2Info, value);
				SetNonPersistentPropertyValue<ZString>(Group2Info, ref group2, value);
				if (!IsValidationSuspended)
				{
					ValidateGroup2();
				}
			}
		}

		public ZPropertyInfo Group2Info
		{
			get { return GetZPropertyInfo(Schema.Group2); }
		}

		public void ValidateGroup2()
		{
			Group2Info.ClearAllNotifications();
			ListValidation.ErrorIfInvalidCode(Group2Info, GroupByList);
		}

		ZString group2;

		#endregion

		#region Group 3

		[CargoWise.ComponentModel.MaxLength(3)]
		public ZString Group3
		{
			get { return group3; }
			set
			{
				CheckMaximumLength(Group3Info, value);
				SetNonPersistentPropertyValue<ZString>(Group3Info, ref group3, value);
				if (!IsValidationSuspended)
				{
					ValidateGroup3();
				}
			}
		}

		public ZPropertyInfo Group3Info
		{
			get { return GetZPropertyInfo(Schema.Group3); }
		}

		public void ValidateGroup3()
		{
			Group3Info.ClearAllNotifications();
			ListValidation.ErrorIfInvalidCode(Group3Info, GroupByList);
		}

		ZString group3;

		#endregion

		#endregion

		#region Lookups

		public CodeDescriptionPairList GroupByList
		{
			get
			{
				if (groupByList == null)
				{
					groupByList = new CodeDescriptionPairList();
					groupByList.AddPair("JTY", Res.GetString("c1bc73eb-5d55-4c1c-9c2c-66d16b9f9763", "Job Type"));
					groupByList.AddPair("CCO", Res.GetString("6ac83d4c-c638-4a8b-b10b-57406ce243e1", "Charge Code"));
					groupByList.AddPair("PRD", Res.GetString("aef358c9-5963-4bd2-8ac3-d320cf4f3d16", "Product Code"));
					groupByList.AddPair("AT1", Res.GetString("fe30132a-d582-4080-acd0-f4efc98dcff1", "Part Attribute 1"));
					groupByList.AddPair("AT2", Res.GetString("8013192a-ce70-4407-a984-977435f772ee", "Part Attribute 2"));
					groupByList.AddPair("AT3", Res.GetString("1f3da0d8-1ff1-43f5-a3fd-194643c00544", "Part Attribute 3"));
					groupByList.AddPair("COM", Res.GetString("f5efddf6-b5f8-48a2-af81-c8224840d36a", "Commodity Code"));
					groupByList.AddPair("DRE", Res.GetString("fda292bb-e435-4cf5-b640-1ff34f9603e3", "Docket Reference"));
					groupByList.AddPair("LCD", Res.GetString("f9158ffb-a822-4254-ae0d-2e9cddf0632a", "Location Description"));
					groupByList.AddPair("LCT", Res.GetString("ac73f49c-42b5-4661-a328-cf05796ce5f8", "Location Type"));
				}

				return groupByList;
			}
		}

		CodeDescriptionPairList groupByList;

		#endregion

		#region Xml Serialisation

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);

			writer.WriteElementString(Schema.Group1, Group1);
			writer.WriteElementString(Schema.Group2, Group2);
			writer.WriteElementString(Schema.Group3, Group3);
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			Group1 = reader.ReadElementString(Schema.Group1);
			Group2 = reader.ReadElementString(Schema.Group2);
			Group3 = reader.ReadElementString(Schema.Group3);
		}

		#endregion
	}
}
