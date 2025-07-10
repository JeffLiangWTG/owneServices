using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.Data;
using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Accounting.Business.XmlSerializers")]
	public class GLPresentationJournalCategoryCollection : CodeDescriptionBoolWithExtraBoolCollection
	{
		public GLPresentationJournalCategoryCollection()
			: base(null, true)
		{
		}

		public GLPresentationJournalCategoryCollection(FallbackLevel fallbacklevel)
			: base(fallbacklevel)
		{
		}

		public GLPresentationJournalCategoryCollection(int codeMaxLength)
			: base(null, codeMaxLength)
		{
		}

		public new GLPresentationJournalCategory this[int i]
		{
			get { return (GLPresentationJournalCategory)Elements[i]; }
		}

		public new GLPresentationJournalCategory AddNew()
		{
			return (GLPresentationJournalCategory)base.AddNew();
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new GLPresentationJournalCategory();
		}

		protected override CodeDescriptionBoolCollection GetNewCollection()
		{
			return new GLPresentationJournalCategoryCollection();
		}

		#region IXmlSerializable Members

		protected override void ReadXmlCore(XmlReader reader)
		{
			reader.Read();
			while (reader.IsStartElement("GLPresentationJournalCategory"))
			{
				var element = (GLPresentationJournalCategory)ElementSerialiser.Deserialize(reader);
				if (!element.SystemDefined)
				{
					Add(element);
				}
				else
				{
					foreach (GLPresentationJournalCategory el in this)
					{
						if (el.Code == element.Code)
						{
							el.Bool2 = element.Bool2;
							el.Bool3 = element.Bool3;
							el.Bool4 = element.Bool4;
							break;
						}
					}
				}
			}
		}

		#endregion

		public CodeDescriptionPair EliminationCategory
		{
			get
			{
				var category = this.ToArray<GLPresentationJournalCategory>().FirstOrDefault(x => x.Bool && x.Bool2);
				if (category != null)
				{
					return new CodeDescriptionPair(category.Code.ToString(), category.Description);
				}
				return null;
			}
		}

		public CodeDescriptionPair ClosingCategory
		{
			get
			{
				var category = this.ToArray<GLPresentationJournalCategory>().FirstOrDefault(x => x.Bool && x.Bool3);
				if (category != null)
				{
					return new CodeDescriptionPair(category.Code.ToString(), category.Description);
				}
				return null;
			}
		}

		public CodeDescriptionPair OpeningCategory
		{
			get
			{
				var category = this.ToArray<GLPresentationJournalCategory>().FirstOrDefault(x => x.Bool && x.Bool4);
				if (category != null)
				{
					return new CodeDescriptionPair(category.Code.ToString(), category.Description);
				}
				return null;
			}
		}

		protected override FallbackLevel CurrentFallbackLevelCore
		{
			get { return base.CurrentFallbackLevelCore; }
			set
			{
				bool changed = CurrentFallbackLevelCore != value;
				base.CurrentFallbackLevelCore = value;
				if (changed)
				{
					fCategoriesInUse = null;
				}
			}
		}

		#region Categories in use

		public string[] CategoriesInUse
		{
			get { return fCategoriesInUse ?? (fCategoriesInUse = LoadCategoriesInUse()); }
		}
		string[] fCategoriesInUse;

		string[] LoadCategoriesInUse()
		{
			return GLPresentationJournalCategoryCollection.GetCategoriesInUse(CurrentFallbackLevel != null ? CurrentFallbackLevel.CompanyPK(false) : Guid.Empty);
		}

		#endregion

		#region Static helper method

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		internal static string[] GetCategoriesInUse(Guid companyPK)
		{
			var result = new List<string>();

			string sql = "select distinct AA_TransactionCategory from dbo.AccGLAggregate" + (companyPK != Guid.Empty ? " join dbo.GlbBranch on GB_PK = AA_GB where GB_GC = @CompanyPK" : "");
			using (DbCommand loadInUse = Db.Connection.Command(sql))
			{
				if (companyPK != Guid.Empty)
				{
					loadInUse.AddParameterBasedOnDbColumn("@CompanyPK", companyPK, GlbBranchSchema.GB_GC);
				}

				using (var reader = loadInUse.ExecuteReader())
				{
					while (reader.Read())
					{
						string cartegory = reader[AccGLAggregateSchema.Constants.AA_TransactionCategory].ToString();
						if (!string.IsNullOrEmpty(cartegory))
						{
							result.Add(cartegory.ToUpper());
						}
					}
				}
			}

			return result.ToArray();
		}

		#endregion
	}
}
