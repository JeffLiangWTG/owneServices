using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.BufferManagement.Integration;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.BufferManagement.Business
{
	class DefaultTagOperationStrategy : ITagOperationStrategy
	{
		ITagOperationResult ITagOperationStrategy.AddTag(ITagable tagable, ITagMagnitude magnitude, bool showSecurityDialog)
		{
			if (TagSecurity.CheckTagAddSecurity(magnitude, showSecurityDialog))
			{
				var factory = magnitude.Factory;
				var tagLink = factory.New<TagLink>();
				tagLink.TGL_TGM_Magnitude = magnitude.PK;
				tagLink.TGL_ParentId = tagable.PK;
				tagLink.TGL_ParentTableCode = tagable.TablePrefix;

				tagLink.RunPreSaveValidation();

				return TagOperationResult.Success(tagLink);
			}
			else
			{
				return TagOperationResult.Failed(TagOperationResult.NoPermissionMessage);
			}
		}

		ITagOperationResult ITagOperationStrategy.RemoveTag(ITagable tagable, ITagMagnitude magnitude, bool showSecurityDialog)
		{
			if (TagSecurity.CheckTagRemoveSecurity(magnitude, showSecurityDialog))
			{
				var factory = magnitude.Factory;
				var tagLink = tagable.TagLinks.ToArray().FirstOrDefault(l => l.TGL_TGM_Magnitude == magnitude.PK);

				if (tagLink != null)
				{
					var tagLinkInRightFactory = factory.Load<TagLink>(tagLink.PK);
					if (tagLinkInRightFactory == null)
					{
						return QueueMemberOperationResult.Failed(TagOperationResult.ObjectNotInDatabase);
					}
					tagLinkInRightFactory.Delete();
					return TagOperationResult.Success(tagLinkInRightFactory);
				}
				else
				{
					return TagOperationResult.NoOperation(Res.GetString("83a3ac41-4bcb-4e97-8f35-dbc52c73af5b", "This item does not have the requested tag"));
				}
			}
			else
			{
				return TagOperationResult.Failed(TagOperationResult.NoPermissionMessage);
			}
		}
	}
}
