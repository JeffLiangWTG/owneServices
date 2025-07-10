using System;
using System.Drawing;
using System.IO;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	[DefaultField("FullName"), WrapperTypeName("Person")]
	public class PersonWrapper : GenericWrapper
	{
		public PersonWrapper(GlbPerson person, BusinessObjectFactory factory)
			: base(person, factory)
		{
			PersonInfo = person ?? throw new ArgumentNullException(nameof(person));
		}

		public GlbPerson PersonInfo { get; }

		public ZString FullName => PersonInfo.PER_FullName;
		public ZString FirstName => PersonInfo.PER_FriendlyName;
		public ZString Title => PersonInfo.PER_NameTitle;
		public ZString City => PersonInfo.PER_City;

		readonly string[] gendersToIgnore = new[] { Core.Constants.Genders.NotSpecified, Core.Constants.Genders.Custom, Core.Constants.Genders.Agender, Core.Constants.Genders.NonBinary };
		public ZString Gender => gendersToIgnore.Any(s => PersonInfo.PER_GenderInternal.Contains(s))
			? ZString.Empty
			: PersonInfo.PER_GenderInternal;

		public ZInt Age => PersonInfo.PER_Age;
		public ZString BirthDate => PersonInfo.PER_BirthDateAndAge_Formatted;
		public ZString MobilePhone => PersonInfo.PER_MobilePhone_Formatted;
		public ZString HomePhone => PersonInfo.PER_HomePhone_Formatted;
		public ZString FaxNum => PersonInfo.PER_FaxNum_Formatted;
		public ZString EmailAddress => PersonInfo.PER_EmailAddress;

		public Image Picture
		{
			get
			{
				if (picture != null && picture.IsDisposed())
				{
					picture = null;
				}

				if (picture == null && PersonInfo.PER_Picture.Length > 0)
				{
					picture = Image.FromStream(new MemoryStream(PersonInfo.PER_Picture));
				}

				return picture;
			}
		}

		Image picture;
	}
}
