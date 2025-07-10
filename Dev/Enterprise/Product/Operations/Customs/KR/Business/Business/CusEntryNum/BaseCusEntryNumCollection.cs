using System;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.KR.Business
{
	public abstract class BaseCusEntryNumCollection<MasterT> : DependentBusinessObjectCollection<CusEntryNumber, MasterT> where MasterT : BusinessObject, ILinkable
	{
		public BaseCusEntryNumCollection(MasterT parent)
			: base(parent)
		{
		}

		public CusEntryNumber this[string index]
		{
			get
			{
				var regex = new Regex(@"^\s*""(?<entryType>[A-Za-z0-9]{3})\s*""$");
				var parameters = regex.Match(index);
				if (parameters.Success)
				{
					string entryType = parameters.Groups["entryType"].Value;
					return this.Cast<CusEntryNumber>().FirstOrDefault(x => x.CE_EntryType == entryType);
				}

				return null;
			}
		}
		public CusEntryNumber GetOrCreateCusEntryNum(ZString entryType)
		{
			var entryNum = this.Cast<CusEntryNumber>().FirstOrDefault(x => x.CE_EntryType == entryType);
			if (entryNum == null)
			{
				entryNum = AddNew();
				entryNum.CE_EntryType = entryType;
			}
			return entryNum;
		}

		public CusEntryNumber GetCusEntryNumWithMatchingVersionNumber(ZString entryType, ZString entryLineReference)
		{
			return this.Cast<CusEntryNumber>().FirstOrDefault(x => x.CE_EntryType == entryType && x.CE_EntryLineReference == entryLineReference);
		}

		public CusEntryNumber GetOrCreateCusEntryNumWithMatchingVersionNumber(ZString entryType, ZString entryLineReference, Func<CusEntryNumber, bool> filter)
		{
			var result = GetCusEntryNumWithMatchingVersionNumber(entryType, entryLineReference);

			if (result != null && filter != null && !filter(result))
			{
				result = null;
			}

			if (result == null)
			{
				result = AddNew();
				result.CE_EntryType = entryType;
				result.CE_EntryLineReference = entryLineReference;
			}
			return result;
		}

		public CusEntryNumber GetCusEntryNumWithMaxVersionNumber(ZString entryType)
		{
			return this.Cast<CusEntryNumber>().Where(x => x.CE_EntryType == entryType).OrderBy(x => x.CE_EntryLineReference).LastOrDefault();
		}

		public bool UpdateCusEntryNumIfExists(ZString entryType, ZString propertyName, IZType valueToUpdate)
		{
			var result = false;
			var numbers = this.Cast<CusEntryNumber>().Where(x => x.CE_EntryType == entryType);
			if (numbers.Count() == 1)
			{
				var entryNum = numbers.First();
				var propertyToBeUpdated = entryNum.ZPropertyInfoHash[propertyName];
				propertyToBeUpdated.Value = valueToUpdate;
				result = true;
			}
			return result;
		}

		protected override SchemaGuidColumn FKSchemaColumnInDependent => CusEntryNumSchema.CE_ParentID;
	}
}
