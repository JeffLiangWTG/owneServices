# Digitization by shipamax

### The code in this solution contains the logic of document data processing after document recognition done by Shipamax machine learning models.

Post parsing logic is implemented as part of CW1 service tasks and may vary depending on document type.

#### There are the following data processing steps supported:
* Organization matching
* Product matching
* Master data matching
* Validation
* Update eDoc
* Notify 3d Party


#### Organization Matching
Matches the data parsed by machine learning model with organization data from CW1 Odyssey database

#### Product matching
Matches commercial invoice product codes information

#### Master data matching
Matches various master data information

#### Validation
Validates document data for correctnes and determines if manual correction step is necessary

#### Update eDoc
Generates UXML from parsed data and calls [eDocs service](https://devops.wisetechglobal.com/wtg/CargoWise/_git/Dev?path=%2FEnterprise%2FServices%2FServiceHost%2FWebAPI%2FControllers%2FShipamaxIntegration%2FEDocsShipamaxController.cs)
to update correspondent eDoc

#### Notify 3d Party
Sends parsed result to a 3d party service. 
